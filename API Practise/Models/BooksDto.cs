namespace API_Practise.Models
{
    public class BooksDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Author { get; set; } = string.Empty;

        public int YearPublished { get; set; }
    }
}
