using System;

public class Client
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
    public string CreatedBy
    {
        get; set;
    }

    public string UpdateBy { get; set; }
}

public class ClientUser
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
    public string CreatedBy
    {
        get; set;
    }

    public string UpdateBy { get; set; }
}