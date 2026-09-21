using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kurs_musik_club.Classes
{
    public class recording_and_mixing
    {
        public int id_recording_and_mixing {  get; set; }
        public double recording_duration {  get; set; }
        public string name_rec {  get; set; }

        public recording_and_mixing(int id_recording_and_mixing, double recording_duration, string name_rec)
        {
            this.id_recording_and_mixing = id_recording_and_mixing;
            this.recording_duration = recording_duration;
            this.name_rec = name_rec;
        }
    }
}
