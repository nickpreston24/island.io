using System.Text;
using System.Text.RegularExpressions;
using CodeMechanic.Advanced.Regex;
using CodeMechanic.Diagnostics;
using CodeMechanic.Types;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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

    [Route("/skyline")]
    public int GetSkyline()
    {
        string json = "[[1, 5, 3], [2, 8, 6], [6, 9, 4], [7, 12, 5]]";
        List<int[]> building_bounds = JsonConvert.DeserializeObject<List<int[]>>(json);

        var buildings = GetBuildings(building_bounds);
        // bounds:
        // var building2 = buildings.FirstOrDefault(x => x.id == 2);
        // // building2.Collisions.Count().Dump("Building 2 collisions");
        // // Console.WriteLine(building2);
        //
        // var building4 = buildings.FirstOrDefault(x => x.id == 4);
        // // building4.Collisions.Count().Dump("Building 4 collisions");
        //
        // var building1 = buildings.FirstOrDefault(x => x.id == 1);
        // // building1.Collisions.Count().Dump("Building 1 collisions");
        //
        // var building3 = buildings.FirstOrDefault(x => x.id == 3);
        // // building3.Collisions.Count().Dump("Building 3 collisions");

        int naive_area = buildings.Sum(building => building.Area);

        // var unoccupied_blocks


        // Console.WriteLine("naive area :>> " + naive_area);

        // Print(naive_area, fifth_grade_math_surface_area, building1
        //     // , new IslandFile() { file_name = "kingbob" }
        // );
        //
        //


        // what if I tried matrix?
        int max_h = buildings.Max(x => x.height);
        int max_x = buildings.FirstOrDefault().largest_x2;

        int[,] sky = new int[max_x, max_h];

        // Print(sky, max_h * max_x);

        sky = PlotSkyline(building_bounds, sky);

        int fifth_grade_math_surface_area = CountEmptyBoxes(building_bounds, sky);
        Print("How I'd tally this thing :>> " + fifth_grade_math_surface_area);

        int actual_sky_dimensions = sky.GetLength(0) * sky.GetLength(1);
        Print("actual sky dimensions " + actual_sky_dimensions); // 72

        return fifth_grade_math_surface_area;
    }

    private int CountEmptyBoxes(List<int[]> building_bounds, int[,] sky)
    {
        // sky.Dump("sky passed in");
        int empty_boxes = 0;
        int full_boxes = 0;

        for (int i = 0; i < sky.GetLength(0); i++)
        {
            for (int j = 0; j < sky.GetLength(1); j++)
            {
                int box_value = sky[i, j];
                Console.WriteLine($"box value from {i}-{i + 1},{j} :>> " + box_value);
                if (box_value == 0)
                    empty_boxes++;
                else
                    full_boxes++;
            }
        }

        // empty_boxes.Dump(nameof(empty_boxes));
        // full_boxes.Dump(nameof(full_boxes));

        return empty_boxes;
    }

    private int[,] PlotSkyline(List<int[]> building_bounds, int[,] sky)
    {
        foreach (var bounds in building_bounds)
        {
            // Print(bounds);
            int x = bounds[0];
            int y = bounds[1];
            int h = bounds[2];

            // Print(x, y, h);
            Print($"from {x} to {y}, height {h}");

            for (int i = x + 1; i <= y; i++)
            {
                for (int j = 1; j <= h; j++)
                {
                    Print($"Plotting box ({i - 1},{j - 1})");
                    // Increment # of buildings existing at this coordinate
                    sky[i - 1, j - 1] = sky[i - 1, j - 1] + 1;
                }
            }
        }

        Print(sky);
        return sky;
    }

    private void Print(params object[] items)
    {
        foreach (var item in items)
        {
            if (item.OverridesToString())
                Console.WriteLine(item);
            // else
            //     item.Dump();
        }
    }

    private List<Building> GetBuildings(List<int[]> arrs)
    {
        var buildings = arrs.Aggregate(new List<Building>(), (list, next) =>
        {
            int index = list.Count;
            var x = arrs[index];
            var building = new Building()
            {
                id = index + 1,
                leftmost_limit = x[0],
                rightmost_limit = x[1],
                height = x[2],
            };
            list.Add(building);
            return list;
        });

        // all buildings are now aware of all buildings.
        foreach (var building in buildings)
        {
            building.buildings = buildings;
        }

        return buildings;
    }
}

public class Building
{
    public int id { get; set; } = -1;
    public int leftmost_limit { get; set; } = -1;
    public int rightmost_limit { get; set; } = -1;
    public int height { get; set; } = -1;

    public List<Building> buildings { get; set; } = new List<Building>();

    // Computed props:
    public int max_height => buildings.Max(x => x.height);
    public int smallest_x1 => buildings.Min(x => x.leftmost_limit);
    public int largest_x2 => buildings.Max(x => x.rightmost_limit);
    public int area_of_the_sky => max_height * (largest_x2 - 0);
    public int Area => height * (rightmost_limit - leftmost_limit);

    public string Name => $"Building {id}";
    public List<Building> Collisions => buildings.Where(x => this.collides_with(x)).ToList();


    public override string ToString()
    {
        return new StringBuilder()
            .AppendLine($"{nameof(Name)}={Name}")
            .AppendLine($"{nameof(area_of_the_sky)}={area_of_the_sky}")
            .AppendLine("## Dimensions")
            .AppendLine($"{nameof(Area)}={Area}")
            .AppendLine($"{nameof(max_height)}={max_height}")
            .AppendLine($"{nameof(leftmost_limit)}={leftmost_limit}")
            .AppendLine($"{nameof(rightmost_limit)}={rightmost_limit}")
            .ToString();
    }

    public bool collides_with(Building building)
    {
        if (this.leftmost_limit.IsWithin(building.leftmost_limit, building.rightmost_limit)) return true;
        if (this.rightmost_limit.IsWithin(building.rightmost_limit, building.leftmost_limit)) return true;

        return false;
    }
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

    public string user_id { get; set; } = string.Empty;
}

public record file_extension
{
    public string ext { get; set; }
}

public static class Extensions
{
    public static bool IsWithin(this int x, int a = 0, int b = 0)
    {
        if (a == b)
        {
            return false;
        }

        return (a <= x && x <= b) || (x >= a && b >= x);
    }

    public static bool OverridesToString(this object obj)
    {
        // This Type or one of its base types has overridden object.ToString()
        return obj.ToString() != obj.GetType().ToString();
    }
}