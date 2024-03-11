using homework_3._4._24.data;

namespace homework_3._4._24.web.Models
{
    public class SimchaViewModel
    {
        public List<Simcha> Simchas { get; set; }
        public List<Contributor> Contributors { get; set; }
        public Contributor Contributor { get; set; }
        public int TotalContributors { get; set; }
        public Simcha Simcha { get; set; }

    }
}
