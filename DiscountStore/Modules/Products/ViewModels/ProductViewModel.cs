namespace DiscountStore.Modules.Products.ViewModels
{
    public class ProductViewModel
    {
        public ProductViewModel(string name, double price, int id)
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