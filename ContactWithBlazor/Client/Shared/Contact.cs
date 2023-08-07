namespace ContactWithBlazor.Client.Shared
{
    public class Contact
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Title { get; set; }
        public DateTimeOffset BirthDate { get; set; }
        public string Description { get; set; }
        public bool MarriageStatus { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }
}
