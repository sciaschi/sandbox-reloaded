public class LoadedPrefab
{
	private string _path;
	private GameObject _prefab;

	public LoadedPrefab( string prefabPath )
	{
		_path = prefabPath;
		_prefab = GameObject.GetPrefab( _path );
	}

	public GameObject Prefab
	{
		get
		{
			if ( !_prefab.IsValid() )
			{
				_prefab = GameObject.GetPrefab( _path );
			}
			return _prefab;
		}
	}
}
