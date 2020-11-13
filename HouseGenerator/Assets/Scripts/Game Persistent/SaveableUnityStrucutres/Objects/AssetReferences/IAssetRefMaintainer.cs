
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public interface IAssetRefMaintainer : IAssetRefHolder
{

    IAssetInitializer GetInitializer();

}

