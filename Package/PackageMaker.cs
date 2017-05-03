using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AssetstorePackageImprter
{
	class PackageMaker : EditorWindow
	{

		[MenuItem("Assets/Packagemaker/Create request files", false, 50)]
		static void Make()
		{
			if( window == null ){
				window = PackageMaker.GetWindow<PackageMaker>();
			}
			
			var path = AssetDatabase.GetAssetPath( Selection.activeObject );
			if( string.IsNullOrEmpty( path ) == false ){
				window.path = path;
				var fullpath = Path.GetFullPath("Assets");
				window.Load(fullpath);
			}
			
			window.ShowPopup();
			window.Focus();
		}
		
		string path = string.Empty;
		int assetId = 0;
		string url = string.Empty;
		bool isAssetStore = true;
		static PackageMaker window;
		
		List<AssetData> assetdataList = new List<AssetData>();
		
		void OnGUI()
		{
			using (var horizonal = new EditorGUILayout.HorizontalScope ("box")) {
				EditorGUILayout.LabelField ("make request asset file");
			}
			using( var horizonal = new EditorGUILayout.HorizontalScope())
			{
				isAssetStore = GUILayout.Toggle (isAssetStore, "IsAssetStore", EditorStyles.miniButton, GUILayout.Width(70));	
				GUILayout.Label ("URL", GUILayout.Width(30));
				if (isAssetStore) {
					GUILayout.Label ("assetstore.unity3d.com/content/", GUILayout.Width (185));
					assetId = EditorGUILayout.IntField (assetId);
				} else {
					url = EditorGUILayout.TextField (url);
				}
			}
			
			using( var horizonal = new EditorGUILayout.HorizontalScope()){
				GUILayout.Label("folderpath:", GUILayout.Width(60));
				GUILayout.Label(path);
				if( GUILayout.Button("Find", GUILayout.Width(40)) ){
					var fullpath = Path.GetFullPath("Assets");
					path = EditorUtility.OpenFolderPanel("package manager", "Assets", string.Empty)
						.Replace(fullpath, "Assets");
					if( string.IsNullOrEmpty(path) == false){
						Load(fullpath);
					}
				}
			}
			
			EditorGUI.BeginDisabledGroup( 
				(isAssetStore && assetId == 0 ) || 
				(isAssetStore == false && url == string.Empty) || 
				assetdataList.Count == 0);
			if( GUILayout.Button("Save")){
				Save();
			}
			EditorGUI.EndDisabledGroup();
			
			foreach( var assetdata in assetdataList ){
				assetdata.isRequestAssets = EditorGUILayout.ToggleLeft( assetdata.path, assetdata.isRequestAssets);
			}
		}
		
		void Save()
		{
			var exportData = new StringBuilder();
			if (isAssetStore) {
				exportData.AppendLine ("/content/" + assetId);
			} else {
				exportData.AppendLine (url);
			}
			foreach( var assetdata in assetdataList ){
				if( assetdata.isRequestAssets ){
					exportData.AppendLine( assetdata.path + "," + assetdata.guid);
				}
			}
			if( Directory.Exists("RequestPackages") == false ){
				Directory.CreateDirectory("RequestPackages");
			}
			string exportFilePath = isAssetStore ? 
						"RequestPackages/ImportPackagesAS" + assetId + ".imp": 
						"RequestPackages/ImportPackages" + url.GetHashCode() + ".imp";
			File.WriteAllText(exportFilePath, exportData.ToString());

			System.Diagnostics.Process.Start ("RequestPackages");
			window.Close ();
		}
		
		void Load(string fullPath)
		{
			assetdataList.Clear();
			
			var filePaths = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
				.Where(item => Path.GetExtension(item) != ".meta")
					.Select(item => item.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).Replace(fullPath, "Assets"));
			foreach( var filePath in filePaths ){
				
				var fileGuid = AssetDatabase.AssetPathToGUID(filePath);
				if( fileGuid == null ){
					continue;
				}
				
				var data = new AssetData(){
					isRequestAssets = true,
					path = filePath,
					guid = fileGuid
				};
				assetdataList.Add(data);
			}
		}
		
		class AssetData
		{
			public bool isRequestAssets = true;
			public string path = string.Empty;
			public string guid = string.Empty;
		}
	}
	
	[InitializeOnLoad]
	class PackageImporter : EditorWindow
	{
		static readonly string onceCheckPath = "Temp/PackageImporter";

		static PackageImporter()
		{
			EditorApplication.delayCall += Init;
		}

		public static void Init()
		{
			if( File.Exists(onceCheckPath) == false )
			{
				var assetdataList = PackageImporter.Load();
				if( assetdataList.Any( item => item.pathList.Count != 0)  ){
						window = PackageImporter.GetWindow<PackageImporter>();
					window.assetdataList = assetdataList;
					Import();
				}
				File.Create(onceCheckPath);
			}

			EditorApplication.delayCall -= Init;
		}
		
		List<AssetData> assetdataList = new List<AssetData>();
		
		static PackageImporter window;
		
		[MenuItem("Assets/Packagemaker/Import request files", false, 51)]
		static void Import()
		{
			if( window == null ){
				window = PackageImporter.GetWindow<PackageImporter>();
			}
			window.assetdataList = PackageImporter.Load ();
			window.ShowPopup();
			window.Focus();
		}
				
		void OnGUI()
		{
			using (var horizonal = new EditorGUILayout.HorizontalScope ("box")) {
				GUILayout.Label ("Find Requested Assets");
				if( GUILayout.Button("Refresh", GUILayout.Width(60)) ){
					assetdataList = PackageImporter.Load();
				}
			}
			if (assetdataList.Any(item => item.pathList.Count != 0) == false) {
				EditorGUILayout.LabelField ("all assets imported");
			}

			foreach( var assetdata in assetdataList ){
				if( assetdata.pathList.Count == 0 ){
					continue;
				}
				
				EditorGUILayout.BeginVertical("box");
				
				using( var horizonal = new EditorGUILayout.HorizontalScope()){
					
					if( GUILayout.Button("A$" , GUILayout.Width(30))){
						if (assetdata.isAssetStore) {
							UnityEditorInternal.AssetStore.Open (assetdata.asseturl);
						} else {
							Application.OpenURL (assetdata.asseturl);
						}
					}
					
					assetdata.isOpenWindow = GUILayout.Toggle(assetdata.isOpenWindow, assetdata.asseturl, EditorStyles.label);
				}
				
				
				if( assetdata.isOpenWindow == true ){
					EditorGUI.indentLevel = 3;
					foreach( var file in assetdata.pathList ){
						EditorGUILayout.LabelField( file);
					}
					EditorGUI.indentLevel = 0;
				}
				EditorGUILayout.EndVertical();
			}
		}
		
		static List<AssetData> Load()
		{
			var assetlist = new List<AssetData>();
			
			var files = Directory.GetFiles("RequestPackages", "ImportPackages*.imp", SearchOption.AllDirectories);
			foreach( var file in files ){
				var text = File.ReadAllText(file);
				var textReader = new System.IO.StringReader(text);
				var url = textReader.ReadLine();
				
				var assetData = new AssetData();
				assetData.asseturl = url;
				assetlist.Add(assetData);
				
				while( textReader.Peek() > -1 ){
					var strs = textReader.ReadLine().Split(',');
					var guid = strs[1];
					var requestFilePath = strs[0];
					
					var filePath = AssetDatabase.GUIDToAssetPath( guid );
					if( string.IsNullOrEmpty( filePath ) || File.Exists(filePath) == false ){
						assetData.pathList.Add(requestFilePath);
					}
				}
			}

			return assetlist;
		}

		void OnFocus()
		{
			assetdataList = PackageImporter.Load();
		}

		class AssetData
		{
			public bool isOpenWindow = false;
			public bool isAssetStore = true;
			private string url;
			public string asseturl{
				get{
					return url;
				}
				set{
					url = value;
					isAssetStore = Regex.IsMatch (url, "/content/\\d");
				}
			}
			public List<string> pathList = new List<string>();
		}
	}

}
