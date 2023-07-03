namespace BulkyBook.Models
{
    public class IndexPageModel
    {
        public List<BookModel> AllBook { get; set; }

        public List<int> SearchBook { get; set; }
        public List<CartModel> Cart { get; set; }
    }
}
