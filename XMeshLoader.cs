// Copyright (c) Microsoft Corporation.  All rights reserved.
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Wew;
using Wew.Media;

static partial class mXMeshLoader
{// ** Tps
	private struct MaterialSpecification									// Specifies how a particular mesh should be shaded
	{	// The difuse color of the material
		public Color MaterialColor;
		// The exponent of the specular color
		public float SpecularPower;
		// The specular color
		public Color SpecularColor;
		// The emissive color
		public Color EmissiveColor;
		// The name of the texture file
		public string TextureFileName;
	}
	public interface iXDataObject											// The base interface type for all data objects found in the .x file
	{   // Returns true if the data object corresponds to some specific object that can
		// be represented visually. For example, a frame or a mesh.
		// Returns false if the data object is simply used as a data member of some other data object.
		// For example, the vertices or materials for a mesh.
		bool IsVisualObject { get; }
		// The template name of the data object's type.
		string DataObjectType { get; }
		// The name of the data object itself (may be empty).
		string Name { get; }
		// The text contained within the body of the data object, once
		// all known data members of the data object have been parsed and
		// removed from the body.
		string Body { get; }
		// The immediate children of the data object.
		Array<iXDataObject> Children { get; }
	}
	public partial class XDataObjectFactory											// A factory class used to create <see cref="IXDataObject" /> instances from text input
	{// ** Tps
		private const RegexOptions DEFAULT_OPTIONS = RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled;
		public class XDataObject : iXDataObject								// Implementation of <see cref="IXDataObject" /> Private implementation ensures that only the <see cref="XDataObjectFactory" /> can create this
		{	readonly string m_sDataObjectType, m_sName, m_sBody;
			Array<iXDataObject> ma_doChildren;
			public Vertex[]? Vertices; public int VertexCant; public eVertexLayout VertexLayout; // Only for meshes
			public Array<int> Indices; public Material Material;
		// ** Ctor/dtor
			// <param name="type">The template name of the type of the data object</param>
			// <param name="name">The name of the data object</param>
			// <param name="body">The remaining unparsed body text of the data object</param>
			// <param name="factory">The factory used to create this object</param>
			// <exception cref="ArgumentNullException">Thrown if the <paramref name="type"/> or <paramref name="factory"/> arguments are null.</exception>
			// <remarks>The factory passed in is used to further parse the object's body text, including
			// resolving references to previously defined objects and templates.</remarks>
			public XDataObject(string type, string name, string body, XDataObjectFactory factory)
			{		System.ArgumentNullException.ThrowIfNull(type);
					System.ArgumentNullException.ThrowIfNull(factory);
				m_sDataObjectType = type; m_sName = name; ma_doChildren = factory.ExtractDataObjectsImpl(ref body); m_sBody = body;
			}
			// ** Props
			public virtual bool IsVisualObject				=>	DataObjectType is "Frame" or "Mesh";
			public virtual string DataObjectType			=>	m_sDataObjectType;
			public virtual string Name						=>	m_sName;
			public virtual string Body						=>	m_sBody;
			public virtual Array<iXDataObject> Children		=>	ma_doChildren;
		}
			// Implementation of <see cref="IXTemplateObject" /> Private implementation ensures that only the <see cref="XDataObjectFactory" /> can create this
			// <param name="name">The name of the template object</param>
			// <param name="body">The remaining unparsed body of the template object</param>
			// <param name="factory">The factory used to create this object</param>
		private class XTemplateObject(string name, string body, XDataObjectFactory factory) : XDataObject("template", name, body, factory)
		{	public override bool IsVisualObject				=>	false;
		}
	// ** Cpos
		private readonly Dictionary<string, XDataObject> objectDictionary = [];
	// ** Estat
		// An expression describing the basic structure of an .x file data object
		private static readonly Regex dataObjectRegex = rgxDataObject();
		// An expression describing a reference to another data object, as found within the body of an .x file data object.
		private static readonly Regex bodyReferenceRegex = rgxBodyReference();
		// An expression describing a UUID declaration of a data object defined in an .x file.
		private static readonly Regex uuidDeclarationRegex = rgxUuidDeclaration();
		// An expression describing the restrictions for a template data object defined in an .x file.
		private static readonly Regex restrictionsDeclarationRegex = rgxRestrictionsDeclaration();
		// An expression describing an individual restriction declaration within a template object defined in an .x file.
		private static readonly Regex restrictionDeclarationRegex = rgxRestrictionDeclaration();
		// Creates an enumeration of data objects represented by the given text.
		// <param name="inputText">The text to parse. After the method returns, this will reference a new string containing all the text that was not parsed.</param>
		// <returns>An enumeration of <see cref="IXDataObject" /> instances represented by the text.</returns>
		public static Array<iXDataObject> ExtractDataObjects(string inputText)
		{	return new XDataObjectFactory().ExtractDataObjectsImpl(ref inputText);
		}
		private static string SingleOrDefault(GroupCollection uuidGroups)	{	return uuidGroups["uuid"].Value;}
		// Processes the given text using the given regex. For every match, the text
		// corresponding to that match is removed from the input text, and the <paramref name="processGroup"/>
		// delegate is invoked to obtain whatever object instance corresponds to the matched text
		// <typeparam name="T">The type of object that will be returned for any matched text</typeparam>
		// <param name="inputText">The text to parse. On return, this will reference to a new string containing only the text that was not parsed into new objects</param>
		// <param name="regex">The regex expression to use to match text</param>
		// <param name="processGroup">The delegate invoked for any matching text, and which returns a new object instance corresponding to the matched text</param>
		// <returns>An enumeration of the objects created by parsing the text</returns>
		private static Array<T> ExtractByRegex<T>(ref string inputText, Regex regex, System.Func<GroupCollection, T> processGroup)
		{
			StringBuilder? bodyBuilder = null;
			Array<T> dataObjects = new (10);
			Match matchObject = regex.Match(inputText);
			int indexCopy = 0;

			while (matchObject.Success)
			{
				// Deferring creation of the StringBuilder has a couple
				// of beneficial effects: the buffer can be pre-sized to
				// a likely reasonable size; more importantly, the code can
				// avoid making a copy of the original input if no sub-objects
				// were found.
				bodyBuilder ??= new StringBuilder(inputText.Length - matchObject.Length);

				_ = bodyBuilder.Append(inputText[indexCopy..matchObject.Index]);
				indexCopy = matchObject.Index + matchObject.Length;

				dataObjects.Add(processGroup(matchObject.Groups));

				matchObject = matchObject.NextMatch();
			}

			if (bodyBuilder is not null)
			{
				_ = bodyBuilder.Append(inputText[indexCopy..]);

				inputText = bodyBuilder.ToString();
			}

			return dataObjects;
		}
		// Extracts a UUID declaration from a data object body if present.
		// <param name="body">The current unparsed .x file body of the data object</param>
		// <returns>The UUID declaration if found, null otherwise.</returns>
		// <remarks>For template objects, be sure to parse the template restrictions
		// before trying to extract the UUID, so that any UUID references found in the
		// template restrictions don't get picked up by this method.</remarks>
		private static string? ExtractUuid(ref string body)
		{	string? uuid = null; Array<string> a_s;

			try
			{	a_s = ExtractByRegex(ref body, uuidDeclarationRegex, SingleOrDefault);
				if (a_s.Count != 0)	uuid = a_s[^1];
			} catch (System.InvalidOperationException exc)
			{	throw new InvalidDataException("Each data object may declare only one UUID", exc);
			}
			return uuid;
		}
	// ** Ctor/dtor
		private XDataObjectFactory()						{ }
	// ** Mets
		// The actual implementation to extract data objects from input text complying with the .x file format.
		// <param name="inputText">The text to parse</param>
		// <returns>An enumeration of <see cref="IXDataObject" /> instances represented within the .x file text.</returns>
		private Array<iXDataObject> ExtractDataObjectsImpl(ref string inputText)
		{	Array<iXDataObject> a_do = default;
			Array<iXDataObject> dataObjects = ExtractByRegex(ref inputText, dataObjectRegex, ExtractDataObject);
			Array<iXDataObject> dataReferences = ExtractByRegex(ref inputText, bodyReferenceRegex, ExtractReference);

			a_do.InsertSpace(0, dataObjects.Count + dataReferences.Count);
			a_do.Write(0, dataObjects.Data, 0, dataObjects.Count); a_do.Write(dataObjects.Count, dataReferences.Data);
			return a_do;
		}
		// Given a regex match for a data object, create a new <see cref="XDataObject" /> instance for the text matched
		// <param name="groups">The match groups for the matched regex expression</param>
		// <returns>A new <see cref="XDataObject" /> instance based on the given regex match</returns>
		private iXDataObject ExtractDataObject(GroupCollection groups)
		{	string type = groups["type"].Value, name = groups["name"].Value;
			string body = groups["body"].Value; string? uuid;
			XDataObject dataObject;

			if (type == "template")
				dataObject = CreateTemplateObject(name, ref body, out uuid);
			else
			{	uuid = ExtractUuid(ref body); dataObject = new XDataObject(type, name, body, this);
			}
			RegisterObject(uuid, dataObject);
			return dataObject;
		}
		private iXDataObject ExtractReference(GroupCollection groups)
		{
			return groups["uuid"].Success ? objectDictionary[groups["uuid"].Value] : (iXDataObject)objectDictionary[groups["name"].Value];
		}
		// Creates an <see cref="XTemplateObject" /> for the given "template" data object
		// type. Parses the UUID, and also the restriction list from the body, matching
		// restriction references to known template objects when possible.
		// <param name="name">The name of the template</param>
		// <param name="body">The remaining unparsed body text for the template</param>
		// <param name="uuid">Receives the declared UUID for the new template object</param>
		// <returns>A new <see cref="XTemplateObject"/> instance</returns>
		private XTemplateObject CreateTemplateObject(string name, ref string body, out string? uuid)
		{	Array<Array<iXDataObject>> aa_doRestric = ExtractByRegex(ref body, restrictionsDeclarationRegex, ExtractRestriction);
			Array<iXDataObject> a_do; bool isOpen = false;

			for (int i = 0; i < aa_doRestric.Count; i++)
			{	a_do = aa_doRestric[i];
				for (int j = 0; j < a_do.Count; j++)
				{	if (a_do[j] is null)
						isOpen = true;
					else if (isOpen)
						throw new InvalidDataException(string.FormatInvariant("Template \"{0}\" mixes open restriction with non-open.", name));
				}
			}
			uuid = ExtractUuid(ref body);
			return new XTemplateObject(name, body, this);
		}
		// For a given restriction declaration, extracts the given templates
		// referenced within the declaration, or null if the declaration is of
		// an open restriction.
		// <param name="groups">The match groups for the matched regex expression</param>
		// <returns>The enumeration of <see cref="IXTemplateObjects" /> represented within the single restriction declaration.</returns>
		// <remarks>The .x file format should not include multiple restriction declarations for
		// a given template object. However, it is theoretically legal to have multiple declarations
		// as long as they don't conflict (i.e. they either all are for an open restriction, or they
		// all list templates for a restricted restriction). This parser will attempt to resolve
		// such theoretically legal multiple declarations if present.</remarks>
		private Array<iXDataObject> ExtractRestriction(GroupCollection groups)
		{	Array<iXDataObject> a_do = new (4);

			if (groups["open"].Success)
				a_do.Add(null);
			else
			{	foreach (Capture reference in groups["ref"].Captures)
				{	Match restrictMatch = restrictionDeclarationRegex.Match(reference.Value); XDataObject? dataObject;

					if (!restrictMatch.Groups["uuid"].Success || (dataObject = RetrieveObject(restrictMatch.Groups["uuid"].Value)) is null)
					{	dataObject = RetrieveObject(restrictMatch.Groups["name"].Value);
					}
					if (dataObject is not null)	a_do.Add(dataObject);
				}
			}
			return a_do;
		}
		// Registers a given object in the factory's object cache
		// <param name="uuid">The object's UUID, if present, null otherwise.</param>
		// <param name="dataObject">The data object itself.</param>
		// <remarks>The object's name will be used as the object key if no UUID is present.
		// Note: the object dictionary will only ever contain the object
		// most recently seen with a given name and/or UUID.  Ideally,
		// a .x file will not use the same name for two different objects,
		// and the specification is not clear on whether that's legal and
		// if so, how to resolve duplicates (especially when it's possible
		// to infer the correct object based on the expected type of object).
		// In this implementation, however, no attempt is made to resolve
		// duplicates intelligently; this may lead to the failure to populate
		// some particular piece of the object tree, when a most recent
		// object of a given name or UUID is not of the expected type.</remarks>
		private void RegisterObject(string? uuid, XDataObject dataObject)
		{
			if (uuid is not null)
			{
#if _DEBUG
				if (objectDictionary.ContainsKey(uuid))
				{
					Debug.WriteLine(string.Format("Key {0} already present", uuid));
				}
#endif
				objectDictionary[uuid] = dataObject;
			}

			if (!string.IsNullOrEmpty(dataObject.Name))
			{
#if _DEBUG
				if (objectDictionary.ContainsKey(dataObject.Name))
				{
					Debug.WriteLine(string.Format("Key {0} already present", dataObject.Name));
				}
#endif
				objectDictionary[dataObject.Name] = dataObject;
			}
		}
		// Retrieves an <see cref="XDataObject" /> with the given key.
		// <param name="key">The key of the object being requested.</param>
		// <returns>The <see cref="XDataObject" /> with the given key in the factory's cache, null if the object is not present.</returns>
		private XDataObject? RetrieveObject(string key)
		{
			return objectDictionary.TryGetValue(key, out XDataObject? value) ? value : null;
		}

		[GeneratedRegex(@"   (?<type>[\w_]+)(:?\s+(?<name>[^\s{]+))?\s*{(?<body>(?>[^{}]+|{(?<bracket>)|}(?<-bracket>))*(?(bracket)(?!)))}", RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace)]
		private static partial Regex rgxDataObject();
		[GeneratedRegex("  {\\s*(?<name>[\\w_]+)?\\s*(?<uuid>\\<\\w{8}-\\w{4}-\\w{4}-\\w{4}-\\w{12}\\>)?\\s*}", RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace)]
		private static partial Regex rgxBodyReference();
		[GeneratedRegex("    (?<uuid>\\<\\w{8}-\\w{4}-\\w{4}-\\w{4}-\\w{12}\\>)", RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace)]
		private static partial Regex rgxUuidDeclaration();
		[GeneratedRegex(@"    (?<=([^\w\s_]+\s*|^\s*))(?<restrict>\[\s*((?<open>\.\.\.)|(?<ref>(?<name>[\w_]+)(\s*(?<uuid>\<\w{8}-\w{4}-\w{4}-\w{4}-\w{12}\>))?)(\s*,\s*(?<ref>(?<name>[\w_]+)(\s*(?<uuid>\<\w{8}-\w{4}-\w{4}-\w{4}-\w{12}\>))?))*)\s*\])", RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace)]
		private static partial Regex rgxRestrictionsDeclaration();
		[GeneratedRegex("    (?<name>[\\w_]+)(\\s*(?<uuid>\\<\\w{8}-\\w{4}-\\w{4}-\\w{4}-\\w{12}\\>))?", RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace)]
		private static partial Regex rgxRestrictionDeclaration();
	}
	private class IndexedMeshNormals
    {	public Array<Vector> NormalVectors;
		public Array<int> NormalIndexMap;
    }
// ** Cpos
	static readonly Regex findArrayCount = rgxFindArrayCount();
	static readonly Regex findVector4F = rgxFindVector4F();
	static readonly Regex findVector3F = rgxFindVector3F();
	static readonly Regex findVector2F = rgxFindVector2F();
	static readonly Regex findScalarF = rgcFindScalarF();
	static readonly Regex findVertexIndex = rgxFindVertexIndex();
// ** Mets
public static void ValidateHeader(string fileHeader)
{
	Regex headerParse = rgxHeaderParse();
	Match m = headerParse.Match(fileHeader);

	if (!m.Success)
	{
		throw new InvalidDataException("Invalid .X file.");
	}

	if (m.Groups.Count != 5)
	{
		// None of the capture groups are optional, so a successful match
		// should always have 5 capture groups
		throw new InvalidDataException("Invalid .X file.");
	}

	if (m.Groups["vermajor"].ToString() != "03")                     // version 3.x supported
		throw new InvalidDataException("Unknown .X file version.");

	if (m.Groups["format"].ToString() != "txt ")
		throw new InvalidDataException("Only text .X files are supported.");
}
public static void LoadMeshes(Wew.Control.cWindow wndDev, string meshDirectory, iXDataObject dataObject)
{	if (dataObject.DataObjectType == "Frame")
	{	iXDataObject[] a_do; iXDataObject childObject;

		// Frame data objects translate to parts with only a transform, and no vertices, materials, etc.
		//ExtractFrameTransformation(dataObject);
		a_do = dataObject.Children.Data!;
		for (int i = 0, iFin = a_do.Length; i < iFin; i++)
		{	childObject = a_do[i];	if (childObject.IsVisualObject)	LoadMeshes(wndDev, meshDirectory, childObject);
		}
 	} else if (dataObject.DataObjectType == "Mesh")
	{		// Mesh data objects inherit transform from their parent, but do have vertices, materials, etc.
			LoadMesh(wndDev, meshDirectory, (XDataObjectFactory.XDataObject)dataObject);
	} else
	{		throw new System.ArgumentException(string.FormatInvariant(
				"Object type \"{0}\" is incorrect. Only Frame or Mesh data objects can be converted to Part instances",
				dataObject.DataObjectType));
	}
}
private static float s_fWrapTc(float fTc)									// Aplicar wrap a texturecoord
{	if (fTc > 1)	return (fTc == (int)fTc ? 1 : fTc % 1);					// Si es entero, es 1 sino la fracción
	if (fTc < 0)	return fTc == (int)fTc ? 0 : 1 + (fTc % 1);				// Si es entero, es 0 sino el complemento a 1
	return fTc;
}
// Helper method to retrieve an immediate child of the
// given <see cref="IXDataObject" /> with the given named type.
// <param name="dataObject">The <see cref="IXDataObject" /> instance the children of which to search</param>
// <param name="type">The template name of the .x file type to retrieve</param>
// <returns>The one child of the given type if present, null if no such child is present.</returns>
// <exception cref="InvalidOperationException">There are more than one child of the given type.</exception>
private static iXDataObject? GetSingleChild(iXDataObject dataObject, string type)
{	iXDataObject[] a_do; iXDataObject obj;

	a_do = dataObject.Children.Data!;
	for (int i = 0, iFin = dataObject.Children.Count; i < iFin; i++)	{	obj = a_do[i];	if (obj.DataObjectType == type)		return obj;}
	return null;
}
//private static Matrix4x4 ExtractFrameTransformation(IXDataObject dataObject)
//{
//	IXDataObject matrixObject = GetSingleChild(dataObject, "FrameTransformMatrix");

//	if (matrixObject is null)
//	{
//		return Matrix4x4.Identity;
//	}

//	string rawMatrixData = matrixObject.Body;

//	Regex matrixData = new Regex("([-\\d\\.,\\s]+);;");
//	Match data = matrixData.Match(rawMatrixData);
//	if(!data.Success)
//		throw new InvalidDataException("Error parsing frame transformation.");

//	string[] values = data.Groups[1].ToString().Split(new char[] { ',' });
//	if(values.Length != 16)
//		throw new InvalidDataException("Error parsing frame transformation.");
//	float[] fvalues = new float[16];
//	for(int n = 0; n < 16; n++)
//	{
//		fvalues[n] = float.Parse(values[n], CultureInfo.InvariantCulture);
//	}

//	return new Matrix4x4(fvalues);
//}
private static Array<MaterialSpecification> LoadMeshMaterialList(iXDataObject dataObject) // Loads the first material for a mesh
{	Array<MaterialSpecification> materials = new (4); iXDataObject[] a_do; iXDataObject child;

	a_do = dataObject.Children.Data!;
	for (int i = 0, iFin = a_do.Length; i < iFin; i++)
	{	child = a_do[i];	if (child.DataObjectType == "Material")		materials.Add(LoadMeshMaterial(child));
	}
	return materials;
}
private static MaterialSpecification LoadMeshMaterial(iXDataObject dataObject)
{	MaterialSpecification m = default;
	int dataOffset = 0;
	Match color = findVector4F.Match(dataObject.Body, dataOffset);

	if(!color.Success)	throw new InvalidDataException("problem reading material color");
	m.MaterialColor.R = float.ParseInvariant(color.Groups[1].ToString());
	m.MaterialColor.G = float.ParseInvariant(color.Groups[2].ToString());
	m.MaterialColor.B = float.ParseInvariant(color.Groups[3].ToString());
	m.MaterialColor.A = 1;//float.Parse(color.Groups[4].ToString(), CultureInfo.InvariantCulture);
	dataOffset = color.Index + color.Length;

	Match power = findScalarF.Match(dataObject.Body, dataOffset);
	if(!power.Success)	throw new InvalidDataException("problem reading material specular color exponent");
	m.SpecularPower = float.ParseInvariant(power.Groups[1].ToString());
	dataOffset = power.Index + power.Length;

	Match specular = findVector3F.Match(dataObject.Body, dataOffset);
	if(!specular.Success)	throw new InvalidDataException("problem reading material specular color");
	m.SpecularColor.R = float.ParseInvariant(specular.Groups[1].ToString());
	m.SpecularColor.G = float.ParseInvariant(specular.Groups[2].ToString());
	m.SpecularColor.B = float.ParseInvariant(specular.Groups[3].ToString());
	m.SpecularColor.A = 1;//
	dataOffset = specular.Index + specular.Length;

	Match emissive = findVector3F.Match(dataObject.Body, dataOffset);
	if(!emissive.Success)	throw new InvalidDataException("problem reading material emissive color");
	m.EmissiveColor.R = float.ParseInvariant(emissive.Groups[1].ToString());
	m.EmissiveColor.G = float.ParseInvariant(emissive.Groups[2].ToString());
	m.EmissiveColor.B = float.ParseInvariant(emissive.Groups[3].ToString());
	m.EmissiveColor.A = 1;//
	//dataOffset = emissive.Index + emissive.Length;

	iXDataObject? filenameObject = GetSingleChild(dataObject, "TextureFilename");

	if (filenameObject is not null)
	{	Regex findFilename = rgxFindFilename();
		Match filename = findFilename.Match(filenameObject.Body);
		if (!filename.Success)	throw new InvalidDataException("problem reading texture filename");
		m.TextureFileName = filename.Groups[1].ToString();
	}

	return m;
}
private static IndexedMeshNormals LoadMeshNormals(iXDataObject dataObject) // Loads the indexed normal vectors for a mesh // <param name="meshNormalData"></param>
{
	IndexedMeshNormals indexedMeshNormals = new ();

	Match normalCount = findArrayCount.Match(dataObject.Body);
	if (!normalCount.Success)
		throw new InvalidDataException("problem reading mesh normals count");

	int normals = int.ParseInvariant(normalCount.Groups[1].Value);
	int dataOffset = normalCount.Index + normalCount.Length;
	indexedMeshNormals.NormalVectors = new Array<Vector>(normals);
	for (int normalIndex = 0; normalIndex < normals; normalIndex++)
	{
		Match normal = findVector3F.Match(dataObject.Body, dataOffset);
			if(!normal.Success)	throw new InvalidDataException("problem reading mesh normal vector");
		
		dataOffset = normal.Index + normal.Length;

		indexedMeshNormals.NormalVectors.Add(
			new Vector(
				float.ParseInvariant(normal.Groups[1].Value),
				float.ParseInvariant(normal.Groups[2].Value),
				float.ParseInvariant(normal.Groups[3].Value)
				));
	}

	Match faceNormalCount = findArrayCount.Match(dataObject.Body, dataOffset);
	if(!faceNormalCount.Success)
		throw new InvalidDataException("problem reading mesh normals count");
            
	indexedMeshNormals.NormalIndexMap = new Array<int>(50);
	int faceCount = int.ParseInvariant(faceNormalCount.Groups[1].Value);
	dataOffset = faceNormalCount.Index + faceNormalCount.Length;
	for(int faceNormalIndex = 0; faceNormalIndex < faceCount; faceNormalIndex++)
	{
		Match normalFace = findVertexIndex.Match(dataObject.Body, dataOffset);
			if(!normalFace.Success)	throw new InvalidDataException("problem reading mesh normal face");
		
		dataOffset = normalFace.Index + normalFace.Length;

		string[] vertexIndexes = normalFace.Groups[2].Value.Split([',']);

		for(int n = 0; n <= vertexIndexes.Length - 3; n ++)
		{
			indexedMeshNormals.NormalIndexMap.Add(int.ParseInvariant(vertexIndexes[0]));
			indexedMeshNormals.NormalIndexMap.Add(int.ParseInvariant(vertexIndexes[1 + n]));
			indexedMeshNormals.NormalIndexMap.Add(int.ParseInvariant(vertexIndexes[2 + n]));
		}
	}

	return indexedMeshNormals;
}
//private static Dictionary<int, ColorF> LoadMeshColors(IXDataObject dataObject) // Loads the per vertex color for a mesh // <param name="vertexColorData"></param>
//{
//	Regex findVertexColor = new Regex("([\\d]+); ([\\d]+\\.[\\d]+);([\\d]+\\.[\\d]+);([\\d]+\\.[\\d]+);([\\d]+\\.[\\d]+);;");

//	Match vertexCount = findArrayCount.Match(dataObject.Body);
//	if(!vertexCount.Success)
//		throw new InvalidDataException("problem reading vertex colors count");

//	Dictionary<int, ColorF> colorDictionary = new Dictionary<int, ColorF>();
//	int verticies = int.Parse(vertexCount.Groups[1].Value, CultureInfo.InvariantCulture);
//	int dataOffset = vertexCount.Index + vertexCount.Length;
//	for(int vertexIndex = 0; vertexIndex < verticies; vertexIndex++)
//	{
//		Match vertexColor = findVertexColor.Match(dataObject.Body, dataOffset);
//		if(!vertexColor.Success)
//			throw new InvalidDataException("problem reading vertex colors");
//		else
//			dataOffset = vertexColor.Index + vertexColor.Length;

//		colorDictionary[int.Parse(vertexColor.Groups[1].Value, CultureInfo.InvariantCulture)] =
//			new ColorF(
//				float.Parse(vertexColor.Groups[2].Value, CultureInfo.InvariantCulture),
//				float.Parse(vertexColor.Groups[3].Value, CultureInfo.InvariantCulture),
//				float.Parse(vertexColor.Groups[4].Value, CultureInfo.InvariantCulture),
//				float.Parse(vertexColor.Groups[5].Value, CultureInfo.InvariantCulture));
//	}

//	return colorDictionary;
//}
private static Point[] LoadMeshTextureCoordinates(iXDataObject dataObject)
{   Match coordinateCount = findArrayCount.Match(dataObject.Body);
        
		if(!coordinateCount.Success)	throw new InvalidDataException("problem reading mesh texture coordinates count");

	int coordinates = int.ParseInvariant(coordinateCount.Groups[1].Value);
	Point[] textureCoordinates = new Point[coordinates];

	int dataOffset = coordinateCount.Index + coordinateCount.Length;
	for(int coordinateIndex = 0; coordinateIndex < coordinates; coordinateIndex++)
	{   Match coordinate = findVector2F.Match(dataObject.Body, dataOffset);

			if(!coordinate.Success)		throw new InvalidDataException("problem reading texture coordinate count");
		dataOffset = coordinate.Index + coordinate.Length;
		textureCoordinates[coordinateIndex] = new Point(
					s_fWrapTc(float.ParseInvariant(coordinate.Groups[1].Value)),
					s_fWrapTc(float.ParseInvariant(coordinate.Groups[2].Value)));
	}
	return textureCoordinates;
}
private static void LoadMesh(Wew.Control.cWindow wndDev, string meshDirectory, XDataObjectFactory.XDataObject doMesh)
{   // ** Load vertex data
	Match vertexCount = findArrayCount.Match(doMesh.Body); int dataOffset;

		if(!vertexCount.Success)	throw new InvalidDataException("problem reading vertex count");
	int verticies = int.ParseInvariant(vertexCount.Groups[1].Value);
	Vector[] vertexList = new Vector[verticies];

	dataOffset = vertexCount.Index + vertexCount.Length;
	for(int vertexIndex = 0; vertexIndex < verticies; vertexIndex++)
	{   Match vertex = findVector3F.Match(doMesh.Body, dataOffset);

			if(!vertex.Success)		throw new InvalidDataException("problem reading vertex");
		dataOffset = vertex.Index + vertex.Length;
		vertexList[vertexIndex] = new Vector(
					float.ParseInvariant(vertex.Groups[1].Value),
					float.ParseInvariant(vertex.Groups[2].Value),
					float.ParseInvariant(vertex.Groups[3].Value)
					);
	}
	// ** Load triangle index data
	Match triangleIndexCount = findArrayCount.Match(doMesh.Body, dataOffset);

	//dataOffset = triangleIndexCount.Index + triangleIndexCount.Length;
		if(!triangleIndexCount.Success)		throw new InvalidDataException("problem reading index count");

	Array<int> triangleIndiciesList = new (50);
	int triangleIndexListCount = int.ParseInvariant(triangleIndexCount.Groups[1].Value);

	dataOffset = triangleIndexCount.Index + triangleIndexCount.Length;
	for(int triangleIndicyIndex = 0; triangleIndicyIndex < triangleIndexListCount; triangleIndicyIndex++)
	{   Match indexEntry = findVertexIndex.Match(doMesh.Body, dataOffset);
            
			if(!indexEntry.Success)		throw new InvalidDataException("problem reading vertex index entry");
		dataOffset = indexEntry.Index + indexEntry.Length;

		int indexEntryCount = int.ParseInvariant(indexEntry.Groups[1].Value);
		string[] vertexIndexes = indexEntry.Groups[2].Value.Split([',']);

			if(indexEntryCount != vertexIndexes.Length)		throw new InvalidDataException("vertex index count does not equal count of indicies found");
		for(int entryIndex = 0; entryIndex <= indexEntryCount - 3; entryIndex++)
		{   triangleIndiciesList.Add(int.ParseInvariant(vertexIndexes[0]));
			triangleIndiciesList.Add(int.ParseInvariant(vertexIndexes[1 + entryIndex].ToString()));
			triangleIndiciesList.Add(int.ParseInvariant(vertexIndexes[2 + entryIndex].ToString()));
		}
	}
	// load mesh colors
	//IXDataObject vertexColorData = GetSingleChild(doMesh, "MeshVertexColors");
	//Dictionary<int, Color> colorDictionary = null;
	//if (vertexColorData)	colorDictionary = LoadMeshColors(vertexColorData);
	// ** Load mesh normals
	iXDataObject? meshNormalData = GetSingleChild(doMesh, "MeshNormals"); Vector[] a_vNorms;

	a_vNorms = new Vector[vertexList.Length];
	if (meshNormalData is not null)
	{	IndexedMeshNormals meshNormals = LoadMeshNormals(meshNormalData);

		for (int n = 0; n < triangleIndiciesList.Count; n++)
			a_vNorms[triangleIndiciesList[n]] = meshNormals.NormalVectors[meshNormals.NormalIndexMap[n]];
	// ** Sin normales: calc
	} else
	{	for (int n = 0, iIdx; n < triangleIndiciesList.Count; n += 3)
		{	Vector vNorm; int iIdx2, iIdx3;

			iIdx = triangleIndiciesList[n]; iIdx2 = triangleIndiciesList[n + 1]; iIdx3 = triangleIndiciesList[n + 2];
			vNorm = (vertexList[iIdx2] - vertexList[iIdx]).Cross(vertexList[iIdx3] - vertexList[iIdx]).GetNormalized();
			a_vNorms[iIdx] = a_vNorms[iIdx2] = a_vNorms[iIdx3] = vNorm;
		}
	}

	// ** Load mesh texture coordinates
	iXDataObject? meshTextureCoordsData = GetSingleChild(doMesh, "MeshTextureCoords");
	Point[]? meshTextureCoords = null;

	if (meshTextureCoordsData is not null)	meshTextureCoords = LoadMeshTextureCoordinates(meshTextureCoordsData);
	// ** Load mesh material
	iXDataObject? meshMaterialsData = GetSingleChild(doMesh, "MeshMaterialList");

	doMesh.Material = Material.Default; doMesh.Material.PixelShader = wndDev.PSStandard;
	if (meshMaterialsData is not null)
	{   MaterialSpecification m = LoadMeshMaterialList(meshMaterialsData)[0]; // only a single material is currently supported
		
		doMesh.Material.Type = eMaterial.Specular; doMesh.Material.Color = m.MaterialColor;
		doMesh.Material.Alpha = 1; doMesh.Material.SpecularPower = (int)m.SpecularPower;
		if (m.SpecularPower == 0)//
		{	doMesh.Material.Type = (m.MaterialColor.R == 0 && m.MaterialColor.G == 0 && m.MaterialColor.B == 0 ? eMaterial.Flat : eMaterial.Light); // AmbientColor
		} else if (m.MaterialColor.R == 0 && m.MaterialColor.G == 0 && m.MaterialColor.B == 0)
		{	doMesh.Material.Color = new Color(.5f, .5f,  .5f, 1);
		}
		if (m.TextureFileName is not null)
			doMesh.Material.Texture = cModelo.GetTextura(Path.Combine(meshDirectory, m.TextureFileName));
	}
	// ** Cfg vertices
	doMesh.Vertices = new Vertex[vertexList.Length];
	for (int n = 0; n < vertexList.Length; n++)
	{	doMesh.Vertices[n].Position = vertexList[n]; doMesh.Vertices[n].Normal = a_vNorms[n]; // Pos y normal
		if (meshTextureCoords is not null)	doMesh.Vertices[n].Texcoord = meshTextureCoords[n]; // Texcoord (opcio)
	}
	doMesh.VertexCant = vertexList.Length;
	doMesh.VertexLayout = (meshTextureCoords is not null ? eVertexLayout.PosNormTex : eVertexLayout.PosNorm);
	doMesh.Indices = triangleIndiciesList;
}

	[GeneratedRegex("([\\d]+);")]
	private static partial Regex rgxFindArrayCount();
	[GeneratedRegex("([-\\d]+\\.[\\d]+);([-\\d]+\\.[\\d]+);([-\\d]+\\.[\\d]+);([-\\d]+\\.[\\d]+);")]
	private static partial Regex rgxFindVector4F();
	[GeneratedRegex("([-\\d]+\\.[\\d]+);([-\\d]+\\.[\\d]+);([-\\d]+\\.[\\d]+);")]
	private static partial Regex rgxFindVector3F();
	[GeneratedRegex("([-\\d]+\\.[\\d]+);([-\\d]+\\.[\\d]+);")]
	private static partial Regex rgxFindVector2F();
	[GeneratedRegex("([-\\d]+\\.[\\d]+);")]
	private static partial Regex rgcFindScalarF();
	[GeneratedRegex("([\\d]+);[\\s]*([\\d,]+)?;")]
	private static partial Regex rgxFindVertexIndex();
	[GeneratedRegex("xof (?<vermajor>\\d\\d)(?<verminor>\\d\\d)(?<format>\\w\\w\\w[\\w\\s])(?<floatsize>\\d\\d\\d\\d)")]
	private static partial Regex rgxHeaderParse();
	[GeneratedRegex("[\\s]+\"([\\\\\\w\\.]+)\";")]
	private static partial Regex rgxFindFilename();
}