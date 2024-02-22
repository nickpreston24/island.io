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
    private static readonly Dictionary<string, string> types_lookup = new Dictionary<string, string>();
    // {
    //     { "ppt", "Document" },
    //     { "docx", "Document" },
    //     { "png", "Photo" },
    //     { "jpg", "Photo" },
    //     { "mp3", "Music" },
    // };

    private static readonly List<string> file_names = new List<string>
    {
        "document.docx",
        "image.jpg",
        "presentation.ppt",
        "music.mp3",
        "photo.png",
    };

    private static readonly List<string> file_types = new List<string>()
    {
        "Document",
        "Image",
        "Document",
        "Audio",
        "Image"
    };

    private static readonly List<int> user_ids = new List<int>
    {
        1, 2, 3
    };

    [HttpGet(Name = "GetIslandFiles")]
    public int[] GetIslandFiles()
    {
        /*
         PROMPT:
             "The function should take three arrays as input: A - user IDs for each file, B - file names, and C -
                 file types."
         
         Q: What can I join these 3 sets on?
         Should User[0] have the first 1 file?  2? 5? Where is the cutoff?
         
         */

        var results_array = solution(user_ids.ToArray(), file_names.ToArray(), file_types.ToArray());
        results_array.Dump("results for Codility");

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

        // Ok, I'm assuming the relationship of file:id:type is by the content of the text for now (I see no other alternative - I could assume 1:1:1, or 1:2:5, etc... but I won't).
        // I see no other relationship, here, sorry.
        // It just doesn't exist in the prompt (unless we're assuming an even ratio), but here goes:

        // I'm assuming user id's are IN the file names, and if they're not, then I'm appending them, so there's a relationship AND so it's somewhat future-proof.
        // Why? In real life, we'd never rely on all 3 arrays to be perfectly ratio'd.  That's just dumb.
        // So, the Extract() function and its Regex will ensure that a legitimate IslandUser and a legitimate IslandFile
        // will be associated with each other NO MATTER THE ARBITRARY LENGTH OF ARRAYS.
        
        
        var island_files = file_names.Select(name =>
            // With Regex.Extract(), we can now relate any file to any type to any user!
            name.Extract<IslandFile>( // From my library, CodeMechanic.Regex
                    // (Regex written by yours truly, with love, at:  https://regex101.com/r/d4k1id/1 )
                    @"(?<file_name>(?<user_id>\d+)?\w+)\.(?<extension>\w+)")
                .FirstOrDefault()
                // ...and back-assign the user and document type
                .With(file => // I wrote this, too.  CodeMechanic.Types
                {
                    file.user = new IslandUser() { user_id = file.user_id.ToInt() }.Dump("user created");
                    file.file_type = lookup_filetype(file.extension);
                })
        );

        
        
        island_files.Dump(nameof(island_files));
        // Then do our counts:


        return result;
    }

    private string lookup_filetype(string extension)
    {
        Console.WriteLine("extension :>> ", extension);

        var is_found = types_lookup.TryGetValue(extension, out string doctype);

        return is_found
            ? doctype.Dump("doctype")
            : throw new Exception($"Could not find matching file type for extension .'{extension}'");
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