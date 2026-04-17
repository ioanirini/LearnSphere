using System;

[Serializable]
public class Manifest
{
    public string asset_base_url;
    public GlbModel[] packages;
}

[Serializable]
public class GlbModel
{
    public string name;
    public string glb_url;
}
