namespace island.io.Controllers;

public class Island
{
    public List<IslandFileGroup> Groups = new List<IslandFileGroup>();
}

public class IslandUser
{
    public int user_id { get; set; }
}

public class IslandFile
{
    public IslandUser user = new IslandUser(); // associate using a join.
    public string file_type { get; set; } = string.Empty;
    public string extension { get; set; } = string.Empty;
    public string file_name { get; set; } = "1234sample.pdf";
    public string user_id = string.Empty;
}

public class IslandFileGroup
{
}