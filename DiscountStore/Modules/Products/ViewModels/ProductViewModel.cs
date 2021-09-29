namespace DiscountStore.Modules.Products.ViewModels
{
    public class ProductViewModel
    {
        public ProductViewModel(int id, string name, double price)
        {
            Name = name;
            Price = price;
            Id = id;
        }
        
        public int Id { get; }
        public string Name { get; }
        public double Price { get; }
    }
}