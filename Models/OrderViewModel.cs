namespace TequilasRestaurant.Models
{
    public class OrderViewModel
    {
        public OrderViewModel()
        {
            OrderItems = new List<OrderItemViewModel>();
            Products = new List<Product>(); 
        }
        public decimal? TotalAmount { get; set; }
        public List<OrderItemViewModel> OrderItems{ get; set; }  
        public IEnumerable<Product> Products { get; set; }
    }
}
