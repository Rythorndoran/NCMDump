using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MusicItemList : ObservableObject
{
    public ObservableCollection<MusicDescriptor> MusicDescriptorList { get; set; }

    public MusicItemList()
    {
        MusicDescriptorList = new ObservableCollection<MusicDescriptor>();
    }
}

