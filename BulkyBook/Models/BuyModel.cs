namespace BulkyBook.Models
{
    public class BuyModel
    {
        public BookModel Book { get; set; } 

        public List<int> inCart { get; set; }
        public List<BookModel> Books { get; set; }
    }
}
