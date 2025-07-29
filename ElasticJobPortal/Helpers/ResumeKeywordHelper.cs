namespace ElasticJobPortal.Helpers
{
    public static class ResumeKeywordHelper
    {
        public static List<string> ExtractSkills(string text)
        {
            // Define a list of common skills you want to extract
            var skillList = new List<string>
        {
            "C", "C#", "C++", "Java", "Python", "PHP", "Laravel", "HTML", "CSS",
            "JavaScript", "SQL", "Bootstrap", "ASP.NET", "React", "Angular", "Node.js"
        };

            var extracted = new List<string>();

            foreach (var skill in skillList)
            {
                if (text.Contains(skill, StringComparison.OrdinalIgnoreCase))
                {
                    extracted.Add(skill);
                }
            }


            return extracted.Distinct().ToList(); // Avoid duplicates
        }

    }

}
