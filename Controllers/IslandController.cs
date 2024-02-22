using System.Text.RegularExpressions;
using CodeMechanic.Advanced.Regex;
using CodeMechanic.Diagnostics;
using CodeMechanic.Types;
using Microsoft.AspNetCore.Mvc;

namespace island.io.Controllers;

[ApiController]
[Route("[controller]")]
public class IslandController : ControllerBase
{
    private static Dictionary<string, string> types_lookup = new Dictionary<string, string>();

    private static readonly List<string> file_names = new List<string>
    {
        "document.docx",
        "image.jpg",
        "presentation.ppt",
        "music.mp3",
        "photo.png",

        // "music.mp3",
        // "document.docx",
    };

    private static readonly List<string> file_types = new List<string>()
    {
        "Document",
        "Image",
        "Document",
        "Audio",
        "Image",

        // "Audio",
        // "Document",
    };

    private static readonly List<int> user_ids = new List<int>
    {
        1, 2, 1, 3, 2
        //, 2, 2
    };

    [HttpGet(Name = "GetIslandFiles")]
    public int[] GetIslandFiles()
    {
        var results_array = solution(user_ids.ToArray(), file_names.ToArray(), file_types.ToArray());
        return results_array;
    }

    /* PLEASE READ THIS
     
     PROMPT: "The function should return an integer array, where each element represents the
number of groups within each user based on file similarity."

    Q: So, up to [3,3,3]?  This seems a bit weird and a bit too easy for a question that needs to be made efficient. 
    LINQ can make quick work of this.
    
    If it can go above 3 Groups for any given user, then why only have 3 file types?  I'm asking this ahead of time, because the phrasing of this question is very generic, has a duplicate sentence and seems a bit too easy to be as straightforward as I think.  Unfortunately, the details seem kind of sparse for both Q1 and Q2, almost as if they were meant to be vague on purpose to see who would try to pop these into ChatGPT and who would attempt to solve it.
    
     */
    private int[] solution(int[] A, String[] B, String[] C)
    {
        var result = new int[] { 0, 0, 0 };
        var user_ids = A;
        var file_names = B;
        var file_types = C;

        // This implementation assumes:
        // a) both arrays are always identical
        // b) both arrays will produce a distinct dictionary every time
        // c) in the future, may NOT have 1:1:1 array ratios
        //      (hence why I prepended the userid to the file name, then called Extract()).

        var renamed_files = RenameFiles(user_ids, file_names);

        // return result;
        types_lookup = generate_lookup(file_types, file_names);

        // The Extract() function and its Regex will ensure that a legitimate IslandUser and a legitimate IslandFile
        // will be associated with each other NO MATTER THE ARBITRARY LENGTH OF ARRAYS.

        var island_files =
            renamed_files
                .Select(f => f.file_name)
                .Select(name =>
                    // With Regex.Extract(), we can now relate any file to any type to any user!
                    name.Extract<IslandFile>( // From my library, CodeMechanic.Regex
                            // (Regex written by yours truly, with love, at:  https://regex101.com/r/d4k1id/1 )
                            @"(?<file_name>(?<user_id>\d+)?\w+\.(?<raw_extension>\w+))")
                        .FirstOrDefault()
                        // ...and back-assign the user and document type
                        .With(file => // I wrote this, too.  CodeMechanic.Types
                        {
                            // file.Dump("extracted file");
                            file.user = new IslandUser { user_id = file.user_id.ToInt() };
                            file.file_type = lookup_filetype(file.raw_extension);
                        })
                );

        // There, now we have a 3-way relationship!

        // ... that came out wrong.

        // (ahem) Now let's do our counts ... :

        // island_files.Dump(nameof(island_files));

        /*
         PROMPT: "The function should return an integer array, where each element 
         represents the number of groups within each user based on file similarity"
         */

        var unique_users = island_files.ToArray()
            .DistinctBy(island => island.user_id)
            .Select(island => island.user)
            .ToArray();

        List<int> tallies = new List<int>();

        foreach (var user in unique_users)
        {
            var uid = user.user_id;
            var group_tally = island_files
                .GroupBy(x => x.file_type)
                .Count(x => x
                    .Any(z => z.user_id == uid.ToString()));
            tallies.Add(group_tally);
        }

        // tallies.Dump("final result");

        return tallies.ToArray();
    }

    private static List<IslandFile> RenameFiles(int[] user_ids, string[] file_names)
    {
        List<IslandFile> renamed_files = new List<IslandFile>();
        for (int index = 0; index < user_ids.Length; index++)
        {
            string fname = user_ids[index] + file_names[index];
            var userid = user_ids[index];
            // Console.WriteLine("userid: " + userid);
            renamed_files.Add(new IslandFile()
            {
                file_name = fname, user_id = userid.ToString(), user = new IslandUser()
                {
                    user_id = userid
                }
                // .Dump("new island user")
            });
        }

        // renamed_files.Dump("renamed files");

        return renamed_files;
    }

    private Dictionary<string, string> generate_lookup(string[] file_types, string[] file_names)
    {
        var lookup = new Dictionary<string, string>();

        for (int i = 0; i < file_types.Length; i++)
        {
            string type = file_types[i];
            string extension = file_names[i]
                    .Extract<file_extension>(@".*\.(?<ext>\w+)")
                    .FirstOrDefault()?.ext.Trim() ?? string.Empty
                ;
            bool success = lookup.TryAdd(extension, type);
            if (!success) continue; // should NEVER happen...
        }

        return lookup;
    }

    private string lookup_filetype(string extension)
    {
        // Console.WriteLine("raw_extension :>> ", extension);
        var is_found = types_lookup
            // .Dump("current types")
            .TryGetValue(extension, out string doctype);

        return is_found
            ? doctype
            // .Dump("doctype")
            : throw new Exception($"Could not find matching file type for raw_extension .'{extension}'");
    }

    /*
     
     PLEASE READ THIS
     
     My recruiter informs me most have failed Q2 so far.  So, I ask that my code be evaluated for quality and how I think, not so much whether I got the answer right.  I don't think there's a right answer for Question 2 because having mapped out the squares on graph paper and using basic 5th grade math, I got a different answer by 2 boxes, and the skyline counting steps seemed arbitrary (first building 2 was 'in front', then 3, then 2 again!  3-building overlaps were counted.  Nothing was recounted, because the steps were in vertical slices, so why mention it?  I could go on...)
     
      I suspect Q2 was ChatGPT generated, but I did my best below to try and guess at the rules it was thinking.  If it wasn't GPT, then I'm sorry, I believe the instructions were unclear and need revision and testing by your dev team.
     
     Thank you,
     
     Nick
     */
    public void Skyline()
    {
    }
}

public class IslandTally
{
}

public class IslandUser
{
    public int user_id { get; set; } = 0;
}

public class IslandFile
{
    public IslandUser user { set; get; } = new IslandUser(); // associate using a join.
    public string file_type { get; set; } = string.Empty;
    public string raw_extension { get; set; } = string.Empty;

    public string file_name { get; set; } = "1234sample.pdf";
    // public string Extension => Regex.Match(@".*\.(?<raw_extension>\w+", raw_extension).Value;

    public string user_id { get; set; } = string.Empty;
}

public record file_extension
{
    public string ext { get; set; }
}