using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAssetReferencer
{

    string RelativePathFromResource { get; set; }

    string AssetName { get; set; }

    string AssetExtension { get; set; }
    
    bool WasAlreadyValidated { get; set; }
    
}
