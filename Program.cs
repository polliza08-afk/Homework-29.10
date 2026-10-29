// See https://aka.ms/new-console-template for more information
using System;

namespace EventExample
{
    //Оголошуємо власний клас для передачі даних події
    public class OrderEventArgs : EventArgs
    {
        public string OrderId { get; set; }
        public decimal TotalAmount { get; set; }
    }

    //Клас видавця події
    public class OrderProcessor
    {
        //Звичайний делегат (публічний, його можна перезаписати зовні)
        public Action<string> OnLegacyNotification;

        //Подія на основі стандартного EventHandler із дженериком
        public event EventHandler<OrderEventArgs> OrderProcessed;

        public void ProcessOrder(string orderId, decimal amount)
        {
            Console.WriteLine($"Обробка замовлення {orderId}...");

            //Імітуємо затримку або роботу
            System.Threading.Thread.Sleep(500);
            //Викликаємо звичайний делегат (якщо він не null)
            OnLegacyNotification?.Invoke($"Замовлення {orderId} оброблено :)");
            //Безпечно викликаємо подію, перевіряючи її на null
            OnOrderProcessed(new OrderEventArgs { OrderId = orderId, TotalAmount = amount });
        }

        //Віртуальний захищений метод для визову події (стандарт .NET для підкласів)
        protected virtual void OnOrderProcessed(OrderEventArgs e)
        {
            //Подія викликається лише зсередини класу-видавця
            OrderProcessed?.Invoke(this, e);
        }
    }

    //Клас підписника події
    public class EmailService
    {
        public void Subscribing(OrderProcessor processor)
        {
            //Підписка на подію за допомогою оператора +=
            processor.OrderProcessed += HandleOrderProcessed;
        }

        private void HandleOrderProcessed(object sender, EventArgs e)
        {
            //Перетворюємо базовий EventArgs у наш кастомний тип
            if (e is OrderEventArgs orderArgs)
            {
                Console.WriteLine($"[EmailService] Надіслано чек на суму {orderArgs.TotalAmount} грн для замовлення {orderArgs.OrderId}");
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            //Створюємо екземпляр видавця
            OrderProcessor processor = new OrderProcessor();
            //Створюємо екземпляр підписника
            EmailService emailService = new EmailService();
            //Підписуємо EmailService на подію
            emailService.Subscribing(processor);
            //Або підписуємося за допомогою лямбда-виразу напряму
            processor.OrderProcessed += (sender, e) =>
            {
                Console.WriteLine($"[Logger] Подію з'їла анонімна функція для замовлення: {e.OrderId}");
            };
            //Небезпека звичайного делегата:
            //зовнішній код може повністю перезаписати делегат, стерши всі попередні підписки!
            processor.OnLegacyNotification = (msg) => Console.WriteLine($"Хакнутий делегат: {msg}");

            //Спроба викликати подію ззовні напряму
            //processor.OrderProcessed.Invoke(null, null); //Видає помилку. Подію не можна викликати ззовні класу

            //Запускаємо процес
            processor.ProcessOrder("ORD-99923", 1500.50m);

            Console.ReadKey();
        }
    }
}
