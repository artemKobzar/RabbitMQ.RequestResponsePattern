namespace ClientConsole.Model
{
    public class Order
    {
        public Guid Id { get; set; } = new Guid();
        public string Name { get; set; }
        public decimal Price { get; set; }
        public DateOnly Created { get; set; }
        //public Customer Customer { get; set; }
    }
}
