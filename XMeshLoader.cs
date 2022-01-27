// Copyright (c) Microsoft Corporation.  All rights reserved.
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Wew;
using Wew.Media;

namespace DirectPaint3D
{
internal static class mXMeshLoader
{// ** Tps
	private struct MaterialSpecification									// Specifies how a particular mesh should be shaded
	{	// The difuse color of the material
		public Color materialColor;
		// The exponent of the specular color
		public float specularPower;
		// The specular color
		public Color specularColor;
		// The emissive color
		public Color emissiveColor;
		// The name of the texture file
		public string textureFileName;
	}
	public interface IXDataObject											// The base interface type for all data objects found in the .x file
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
		Array<IXDataObject> Children { get; }
	}
	public class XDataObjectFactory											// A factory class used to create <see cref="IXDataObject" /> instances from text input
	{// ** Tps
		private const RegexOptions defaultOptions = RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled;
		public class XDataObject : IXDataObject								// Implementation of <see cref="IXDataObject" /> Private implementation ensures that only the <see cref="XDataObjectFactory" /> can create this
		{	string m_sDataObjectType, m_sName, m_sBody;
			Array<IXDataObject> ma_doChildren;
			public Vertex[] Vertices; public int VertexCant; public c3DModel.eVertexLayout VertexLayout; // Only for meshes
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
			{		if (type == null)	throw new System.ArgumentNullException("type");
					if (factory == null)	throw new System.ArgumentNullException("factory");
				m_sDataObjectType = type; m_sName = name; ma_doChildren = factory.ExtractDataObjectsImpl(ref body); m_sBody = body;
			}
		// ** Props
			public virtual bool IsVisualObject
			{	get											{	return DataObjectType == "Frame" || DataObjectType == "Mesh";}
			}
			public virtual string DataObjectType
			{	get											{	return m_sDataObjectType;}
			}
			public virtual string Name
			{	get											{	return m_sName;}
			}
			public virtual string Body
			{	get											{	return m_sBody;}
			}
			public virtual Array<IXDataObject> Children
			{	get											{	return ma_doChildren;}
			}
		}
		private class XTemplateObject : XDataObject							// Implementation of <see cref="IXTemplateObject" /> Private implementation ensures that only the <see cref="XDataObjectFactory" /> can create this
		{	// <param name="name">The name of the template object</param>
			// <param name="body">The remaining unparsed body of the template object</param>
			// <param name="factory">The factory used to create this object</param>
			public XTemplateObject(string name, string body, XDataObjectFactory factory) : base("template", name, body, factory)	{}
			public override bool IsVisualObject	{	get { return false; } }
		}
	// ** Cpos
		private Dictionary<string, XDataObject> objectDictionary = new Dictionary<string, XDataObject>();
	// ** Estat
		// An expression describing the basic structure of an .x file data object
		private static Regex dataObjectRegex = new Regex("   (?<type>[\\w_]+)"
			+ "(:?\\s+(?<name>[^\\s{]+))?\\s*"
			+ "{(?<body>"
			+ "(?>"
			+ "[^{}]+|"
			+ "{(?<bracket>)|"
			+ "}(?<-bracket>)"
			+ ")*"
			+ "(?(bracket)(?!))"
			+ ")}"
			, defaultOptions);
		// An expression describing a reference to another data object, as found within the body of an .x file data object.
		private static Regex bodyReferenceRegex = new Regex(
			"  {\\s*(?<name>[\\w_]+)?\\s*(?<uuid>\\<\\w{8}-\\w{4}-\\w{4}-\\w{4}-\\w{12}\\>)?\\s*}", defaultOptions);
		// An expression describing a UUID declaration of a data object defined in an .x file.
		private static Regex uuidDeclarationRegex = new Regex(
			"    (?<uuid>\\<\\w{8}-\\w{4}-\\w{4}-\\w{4}-\\w{12}\\>)", defaultOptions);
		// An expression describing the restrictions for a template data object defined in an .x file.
		private static Regex restrictionsDeclarationRegex = new Regex(
			"    (?<=([^\\w\\s_]+\\s*|^\\s*))"
			+ "(?<restrict>\\[\\s*"
			+ "("
			+ "(?<open>\\.\\.\\.)|"
			+ "(?<ref>(?<name>[\\w_]+)(\\s*(?<uuid>\\<\\w{8}-\\w{4}-\\w{4}-\\w{4}-\\w{12}\\>))?)"
			+ "(\\s*,\\s*"
			+ "(?<ref>(?<name>[\\w_]+)(\\s*(?<uuid>\\<\\w{8}-\\w{4}-\\w{4}-\\w{4}-\\w{12}\\>))?)"
			+ ")*"
			+ ")"
			+ "\\s*\\])", defaultOptions | RegexOptions.ExplicitCapture);
		// An expression describing an individual restriction declaration within a template object defined in an .x file.
		private static Regex restrictionDeclarationRegex = new Regex(
			"    (?<name>[\\w_]+)(\\s*(?<uuid>\\<\\w{8}-\\w{4}-\\w{4}-\\w{4}-\\w{12}\\>))?", defaultOptions);
		// Creates an enumeration of data objects represented by the given text.
		// <param name="inputText">The text to parse. After the method returns, this will reference a new string containing all the text that was not parsed.</param>
		// <returns>An enumeration of <see cref="IXDataObject" /> instances represented by the text.</returns>
		public static Array<IXDataObject> ExtractDataObjects(string inputText)
		{	return (new XDataObjectFactory()).ExtractDataObjectsImpl(ref inputText);
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
			StringBuilder bodyBuilder = null;
			Array<T> dataObjects = new Array<T>(10);
			Match matchObject = regex.Match(inputText);
			int indexCopy = 0;

			while (matchObject.Success)
			{
				// Deferring creation of the StringBuilder has a couple
				// of beneficial effects: the buffer can be pre-sized to
				// a likely reasonable size; more importantly, the code can
				// avoid making a copy of the original input if no sub-objects
				// were found.
				if (bodyBuilder == null)
				{
					bodyBuilder = new StringBuilder(inputText.Length - matchObject.Length);
				}

				bodyBuilder.Append(inputText.Substring(indexCopy, matchObject.Index - indexCopy));
				indexCopy = matchObject.Index + matchObject.Length;

				dataObjects.Add(processGroup(matchObject.Groups));

				matchObject = matchObject.NextMatch();
			}

			if (bodyBuilder != null)
			{
				bodyBuilder.Append(inputText.Substring(indexCopy));

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
		private static string ExtractUuid(ref string body)
		{	string uuid = null; Array<string> a_s;

			try
			{	a_s = ExtractByRegex(ref body, uuidDeclarationRegex, SingleOrDefault);
				if (a_s.Count != 0)	uuid = a_s[a_s.Count - 1];
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
		private Array<IXDataObject> ExtractDataObjectsImpl(ref string inputText)
		{	Array<IXDataObject> a_do = default(Array<IXDataObject>);
			Array<IXDataObject> dataObjects = ExtractByRegex(ref inputText, dataObjectRegex, ExtractDataObject);
			Array<IXDataObject> dataReferences = ExtractByRegex(ref inputText, bodyReferenceRegex, ExtractReference);

			a_do.InsertSpace(0, dataObjects.Count + dataReferences.Count);
			a_do.Write(0, dataObjects.Data, 0, dataObjects.Count); a_do.Write(dataObjects.Count, dataReferences.Data);
			return a_do;
		}
		// Given a regex match for a data object, create a new <see cref="XDataObject" /> instance for the text matched
		// <param name="groups">The match groups for the matched regex expression</param>
		// <returns>A new <see cref="XDataObject" /> instance based on the given regex match</returns>
		private IXDataObject ExtractDataObject(GroupCollection groups)
		{	string type = groups["type"].Value, name = groups["name"].Value;
			string body = groups["body"].Value, uuid;
			XDataObject dataObject;

			if (type == "template")
				dataObject = CreateTemplateObject(name, ref body, out uuid);
			else
			{	uuid = ExtractUuid(ref body); dataObject = new XDataObject(type, name, body, this);
			}
			RegisterObject(uuid, dataObject);
			return dataObject;
		}
		private IXDataObject ExtractReference(GroupCollection groups)
		{
			if (groups["uuid"].Success)		return objectDictionary[groups["uuid"].Value];
			return objectDictionary[groups["name"].Value];
		}
		// Creates an <see cref="XTemplateObject" /> for the given "template" data object
		// type. Parses the UUID, and also the restriction list from the body, matching
		// restriction references to known template objects when possible.
		// <param name="name">The name of the template</param>
		// <param name="body">The remaining unparsed body text for the template</param>
		// <param name="uuid">Receives the declared UUID for the new template object</param>
		// <returns>A new <see cref="XTemplateObject"/> instance</returns>
		private XDataObject CreateTemplateObject(string name, ref string body, out string uuid)
		{	Array<Array<IXDataObject>> aa_doRestric = ExtractByRegex(ref body, restrictionsDeclarationRegex, ExtractRestriction);
			Array<IXDataObject> a_do; bool isOpen = false;

			for (int i = 0; i < aa_doRestric.Count; i++)
			{	a_do = aa_doRestric[i];
				for (int j = 0; j < a_do.Count; j++)
				{	if (a_do[j] == null)
						isOpen = true;
					else if (isOpen)
						throw new InvalidDataException(
							string.Format(CultureInfo.InvariantCulture, "Template \"{0}\" mixes open restriction with non-open.", name));
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
		private Array<IXDataObject> ExtractRestriction(GroupCollection groups)
		{	Array<IXDataObject> a_do = new Array<IXDataObject>(4);

			if (groups["open"].Success)
				a_do.Add(null);
			else
			{	foreach (Capture reference in groups["ref"].Captures)
				{	Match restrictMatch = restrictionDeclarationRegex.Match(reference.Value); XDataObject dataObject;

					if (!restrictMatch.Groups["uuid"].Success || (dataObject = RetrieveObject(restrictMatch.Groups["uuid"].Value)) == null)
					{	dataObject = RetrieveObject(restrictMatch.Groups["name"].Value);
					}
					if (dataObject != null)	a_do.Add(dataObject);
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
		private void RegisterObject(string uuid, XDataObject dataObject)
		{
			if (uuid != null)
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
		private XDataObject RetrieveObject(string key)
		{
			if (objectDictionary.ContainsKey(key))
			{
				return objectDictionary[key];
			}

			return null;
		}
	}
	private class IndexedMeshNormals
    {	public Array<Vector> normalVectors;
		public Array<int> normalIndexMap;
    }
// ** Cpos
	static Regex findArrayCount = new Regex("([\\d]+);");
	static Regex findVector4F = new Regex("([-\\d]+\\.[\\d]+);([-\\d]+\\.[\\d]+);([-\\d]+\\.[\\d]+);([-\\d]+\\.[\\d]+);");
	static Regex findVector3F = new Regex("([-\\d]+\\.[\\d]+);([-\\d]+\\.[\\d]+);([-\\d]+\\.[\\d]+);");
	static Regex findVector2F = new Regex("([-\\d]+\\.[\\d]+);([-\\d]+\\.[\\d]+);");
	static Regex findScalarF = new Regex("([-\\d]+\\.[\\d]+);");
	static Regex findVertexIndex = new Regex("([\\d]+);[\\s]*([\\d,]+)?;");
// ** Mets
public static void ValidateHeader(string fileHeader)
{
	Regex headerParse = new Regex("xof (?<vermajor>\\d\\d)(?<verminor>\\d\\d)(?<format>\\w\\w\\w[\\w\\s])(?<floatsize>\\d\\d\\d\\d)");
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
public static void LoadMeshes(Wew.Control.cWindow wndDev, string meshDirectory, IXDataObject dataObject)
{	if (dataObject.DataObjectType == "Frame")
	{	IXDataObject[] a_do; IXDataObject childObject;

		// Frame data objects translate to parts with only a transform, and no vertices, materials, etc.
		ExtractFrameTransformation(dataObject);
		a_do = dataObject.Children.Data;
		for (int i = 0, iFin = a_do.Length; i < iFin; i++)
		{	childObject = a_do[i];	if (childObject.IsVisualObject)	LoadMeshes(wndDev, meshDirectory, childObject);
		}
 	} else if (dataObject.DataObjectType == "Mesh")
	{		// Mesh data objects inherit transform from their parent, but do have vertices, materials, etc.
			LoadMesh(wndDev, meshDirectory, (XDataObjectFactory.XDataObject)dataObject);
	} else
	{		throw new System.ArgumentException(string.Format(CultureInfo.InvariantCulture,
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
private static IXDataObject GetSingleChild(IXDataObject dataObject, string type)
{	IXDataObject[] a_do; IXDataObject obj;

	a_do = dataObject.Children.Data;
	for (int i = 0, iFin = dataObject.Children.Count; i < iFin; i++)	{	obj = a_do[i];	if (obj.DataObjectType == type)		return obj;}
	return null;
}
private static Matrix4x4 ExtractFrameTransformation(IXDataObject dataObject)
{
	IXDataObject matrixObject = GetSingleChild(dataObject, "FrameTransformMatrix");

	if (matrixObject == null)
	{
		return Matrix4x4.Identity;
	}

	string rawMatrixData = matrixObject.Body;

	Regex matrixData = new Regex("([-\\d\\.,\\s]+);;");
	Match data = matrixData.Match(rawMatrixData);
	if(!data.Success)
		throw new InvalidDataException("Error parsing frame transformation.");

	string[] values = data.Groups[1].ToString().Split(new char[] { ',' });
	if(values.Length != 16)
		throw new InvalidDataException("Error parsing frame transformation.");
	float[] fvalues = new float[16];
	for(int n = 0; n < 16; n++)
	{
		fvalues[n] = float.Parse(values[n], CultureInfo.InvariantCulture);
	}

	return new Matrix4x4(fvalues);
}
private static Array<MaterialSpecification> LoadMeshMaterialList(IXDataObject dataObject) // Loads the first material for a mesh
{	Array<MaterialSpecification> materials = new Array<MaterialSpecification>(4); IXDataObject[] a_do; IXDataObject child;

	a_do = dataObject.Children.Data;
	for (int i = 0, iFin = a_do.Length; i < iFin; i++)
	{	child = a_do[i];	if (child.DataObjectType == "Material")		materials.Add(LoadMeshMaterial(child));
	}
	return materials;
}
private static MaterialSpecification LoadMeshMaterial(IXDataObject dataObject)
{	MaterialSpecification m = default(MaterialSpecification);
	int dataOffset = 0;
	Match color = findVector4F.Match(dataObject.Body, dataOffset);

	if(!color.Success)	throw new InvalidDataException("problem reading material color");
	m.materialColor.R = float.Parse(color.Groups[1].ToString(), CultureInfo.InvariantCulture);
	m.materialColor.G = float.Parse(color.Groups[2].ToString(), CultureInfo.InvariantCulture);
	m.materialColor.B = float.Parse(color.Groups[3].ToString(), CultureInfo.InvariantCulture);
	m.materialColor.A = 1;//float.Parse(color.Groups[4].ToString(), CultureInfo.InvariantCulture);
	dataOffset = color.Index + color.Length;

	Match power = findScalarF.Match(dataObject.Body, dataOffset);
	if(!power.Success)	throw new InvalidDataException("problem reading material specular color exponent");
	m.specularPower = float.Parse(power.Groups[1].ToString(), CultureInfo.InvariantCulture);
	dataOffset = power.Index + power.Length;

	Match specular = findVector3F.Match(dataObject.Body, dataOffset);
	if(!specular.Success)	throw new InvalidDataException("problem reading material specular color");
	m.specularColor.R = float.Parse(specular.Groups[1].ToString(), CultureInfo.InvariantCulture);
	m.specularColor.G = float.Parse(specular.Groups[2].ToString(), CultureInfo.InvariantCulture);
	m.specularColor.B = float.Parse(specular.Groups[3].ToString(), CultureInfo.InvariantCulture);
	m.specularColor.A = 1;//
	dataOffset = specular.Index + specular.Length;

	Match emissive = findVector3F.Match(dataObject.Body, dataOffset);
	if(!emissive.Success)	throw new InvalidDataException("problem reading material emissive color");
	m.emissiveColor.R = float.Parse(emissive.Groups[1].ToString(), CultureInfo.InvariantCulture);
	m.emissiveColor.G = float.Parse(emissive.Groups[2].ToString(), CultureInfo.InvariantCulture);
	m.emissiveColor.B = float.Parse(emissive.Groups[3].ToString(), CultureInfo.InvariantCulture);
	m.emissiveColor.A = 1;//
	dataOffset = emissive.Index + emissive.Length;

	IXDataObject filenameObject = GetSingleChild(dataObject, "TextureFilename");

	if (filenameObject != null)
	{	Regex findFilename = new Regex("[\\s]+\"([\\\\\\w\\.]+)\";");
		Match filename = findFilename.Match(filenameObject.Body);
		if (!filename.Success)	throw new InvalidDataException("problem reading texture filename");
		m.textureFileName = filename.Groups[1].ToString();
	}

	return m;
}
private static IndexedMeshNormals LoadMeshNormals(IXDataObject dataObject) // Loads the indexed normal vectors for a mesh // <param name="meshNormalData"></param>
{
	IndexedMeshNormals indexedMeshNormals = new IndexedMeshNormals();

	Match normalCount = findArrayCount.Match(dataObject.Body);
	if (!normalCount.Success)
		throw new InvalidDataException("problem reading mesh normals count");

	int normals = int.Parse(normalCount.Groups[1].Value, CultureInfo.InvariantCulture);
	int dataOffset = normalCount.Index + normalCount.Length;
	indexedMeshNormals.normalVectors = new Array<Vector>(normals);
	for (int normalIndex = 0; normalIndex < normals; normalIndex++)
	{
		Match normal = findVector3F.Match(dataObject.Body, dataOffset);
		if(!normal.Success)
			throw new InvalidDataException("problem reading mesh normal vector");
		else
			dataOffset = normal.Index + normal.Length;

		indexedMeshNormals.normalVectors.Add(
			new Vector(
				float.Parse(normal.Groups[1].Value, CultureInfo.InvariantCulture),
				float.Parse(normal.Groups[2].Value, CultureInfo.InvariantCulture),
				float.Parse(normal.Groups[3].Value, CultureInfo.InvariantCulture)
				));
	}

	Match faceNormalCount = findArrayCount.Match(dataObject.Body, dataOffset);
	if(!faceNormalCount.Success)
		throw new InvalidDataException("problem reading mesh normals count");
            
	indexedMeshNormals.normalIndexMap = new Array<int>(50);
	int faceCount = int.Parse(faceNormalCount.Groups[1].Value, CultureInfo.InvariantCulture);
	dataOffset = faceNormalCount.Index + faceNormalCount.Length;
	for(int faceNormalIndex = 0; faceNormalIndex < faceCount; faceNormalIndex++)
	{
		Match normalFace = findVertexIndex.Match(dataObject.Body, dataOffset);
		if(!normalFace.Success)
			throw new InvalidDataException("problem reading mesh normal face");
		else
			dataOffset = normalFace.Index + normalFace.Length;

		string[] vertexIndexes = normalFace.Groups[2].Value.Split(new char[] { ',' });

		for(int n = 0; n <= vertexIndexes.Length - 3; n ++)
		{
			indexedMeshNormals.normalIndexMap.Add(int.Parse(vertexIndexes[0], CultureInfo.InvariantCulture));
			indexedMeshNormals.normalIndexMap.Add(int.Parse(vertexIndexes[1 + n], CultureInfo.InvariantCulture));
			indexedMeshNormals.normalIndexMap.Add(int.Parse(vertexIndexes[2 + n], CultureInfo.InvariantCulture));
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
private static Point[] LoadMeshTextureCoordinates(IXDataObject dataObject)
{   Match coordinateCount = findArrayCount.Match(dataObject.Body);
        
		if(!coordinateCount.Success)	throw new InvalidDataException("problem reading mesh texture coordinates count");

	int coordinates = int.Parse(coordinateCount.Groups[1].Value, CultureInfo.InvariantCulture);
	Point[] textureCoordinates = new Point[coordinates];

	int dataOffset = coordinateCount.Index + coordinateCount.Length;
	for(int coordinateIndex = 0; coordinateIndex < coordinates; coordinateIndex++)
	{   Match coordinate = findVector2F.Match(dataObject.Body, dataOffset);

			if(!coordinate.Success)		throw new InvalidDataException("problem reading texture coordinate count");
		dataOffset = coordinate.Index + coordinate.Length;
		textureCoordinates[coordinateIndex] = new Point(
					s_fWrapTc(float.Parse(coordinate.Groups[1].Value, CultureInfo.InvariantCulture)),
					s_fWrapTc(float.Parse(coordinate.Groups[2].Value, CultureInfo.InvariantCulture)));
	}
	return textureCoordinates;
}
private static void LoadMesh(Wew.Control.cWindow wndDev, string meshDirectory, XDataObjectFactory.XDataObject doMesh)
{   // ** Load vertex data
	Match vertexCount = findArrayCount.Match(doMesh.Body); int dataOffset = 0;

		if(!vertexCount.Success)	throw new InvalidDataException("problem reading vertex count");
	int verticies = int.Parse(vertexCount.Groups[1].Value, CultureInfo.InvariantCulture);
	Vector[] vertexList = new Vector[verticies];

	dataOffset = vertexCount.Index + vertexCount.Length;
	for(int vertexIndex = 0; vertexIndex < verticies; vertexIndex++)
	{   Match vertex = findVector3F.Match(doMesh.Body, dataOffset);

			if(!vertex.Success)		throw new InvalidDataException("problem reading vertex");
		dataOffset = vertex.Index + vertex.Length;
		vertexList[vertexIndex] = new Vector(
					float.Parse(vertex.Groups[1].Value, CultureInfo.InvariantCulture),
					float.Parse(vertex.Groups[2].Value, CultureInfo.InvariantCulture),
					float.Parse(vertex.Groups[3].Value, CultureInfo.InvariantCulture)
					);
	}
	// ** Load triangle index data
	Match triangleIndexCount = findArrayCount.Match(doMesh.Body, dataOffset);

	dataOffset = triangleIndexCount.Index + triangleIndexCount.Length;
		if(!triangleIndexCount.Success)		throw new InvalidDataException("problem reading index count");

	Array<int> triangleIndiciesList = new Array<int>(50);
	int triangleIndexListCount = int.Parse(triangleIndexCount.Groups[1].Value, CultureInfo.InvariantCulture);

	dataOffset = triangleIndexCount.Index + triangleIndexCount.Length;
	for(int triangleIndicyIndex = 0; triangleIndicyIndex < triangleIndexListCount; triangleIndicyIndex++)
	{   Match indexEntry = findVertexIndex.Match(doMesh.Body, dataOffset);
            
			if(!indexEntry.Success)		throw new InvalidDataException("problem reading vertex index entry");
		dataOffset = indexEntry.Index + indexEntry.Length;

		int indexEntryCount = int.Parse(indexEntry.Groups[1].Value, CultureInfo.InvariantCulture);
		string[] vertexIndexes = indexEntry.Groups[2].Value.Split(new char[] { ',' });

			if(indexEntryCount != vertexIndexes.Length)		throw new InvalidDataException("vertex index count does not equal count of indicies found");
		for(int entryIndex = 0; entryIndex <= indexEntryCount - 3; entryIndex++)
		{   triangleIndiciesList.Add(int.Parse(vertexIndexes[0], CultureInfo.InvariantCulture));
			triangleIndiciesList.Add(int.Parse(vertexIndexes[1 + entryIndex].ToString(), CultureInfo.InvariantCulture));
			triangleIndiciesList.Add(int.Parse(vertexIndexes[2 + entryIndex].ToString(), CultureInfo.InvariantCulture));
		}
	}
	// load mesh colors
	//IXDataObject vertexColorData = GetSingleChild(doMesh, "MeshVertexColors");
	//Dictionary<int, Color> colorDictionary = null;
	//if (vertexColorData)	colorDictionary = LoadMeshColors(vertexColorData);
	// ** Load mesh normals
	IXDataObject meshNormalData = GetSingleChild(doMesh, "MeshNormals"); Vector[] a_vNorms = null;

	a_vNorms = new Vector[vertexList.Length];
	if (meshNormalData != null)
	{	IndexedMeshNormals meshNormals = LoadMeshNormals(meshNormalData);

		for (int n = 0; n < triangleIndiciesList.Count; n++)
			a_vNorms[triangleIndiciesList[n]] = meshNormals.normalVectors[meshNormals.normalIndexMap[n]];
	// ** Sin normales: calc
	} else
	{	for (int n = 0, iIdx = 0; n < triangleIndiciesList.Count; n += 3)
		{	Vector vNorm; int iIdx2, iIdx3;

			iIdx = triangleIndiciesList[n]; iIdx2 = triangleIndiciesList[n + 1]; iIdx3 = triangleIndiciesList[n + 2];
			vNorm = ((vertexList[iIdx3] - vertexList[iIdx]) * (vertexList[iIdx2] - vertexList[iIdx])).GetNormalized();
			a_vNorms[iIdx] = a_vNorms[iIdx2] = a_vNorms[iIdx3] = vNorm;
		}
	}

	// ** Load mesh texture coordinates
	IXDataObject meshTextureCoordsData = GetSingleChild(doMesh, "MeshTextureCoords");
	Point[] meshTextureCoords = null;

	if (meshTextureCoordsData != null)	meshTextureCoords = LoadMeshTextureCoordinates(meshTextureCoordsData);
	// ** Load mesh material
	IXDataObject meshMaterialsData = GetSingleChild(doMesh, "MeshMaterialList");

	doMesh.Material = Material.Default; doMesh.Material.PixelShader = wndDev.PSStandard;
	if (meshMaterialsData != null)
	{   MaterialSpecification m = LoadMeshMaterialList(meshMaterialsData)[0]; // only a single material is currently supported
		
		doMesh.Material.Type = eMaterial.Specular; doMesh.Material.Color = m.materialColor;
		doMesh.Material.Alpha = 1; doMesh.Material.SpecularPower = (int)m.specularPower;
		if (m.specularPower == 0)//
		{	doMesh.Material.Type = (m.materialColor.R == 0 && m.materialColor.G == 0 && m.materialColor.B == 0 ? eMaterial.Flat : eMaterial.Light); // AmbientColor
		} else if (m.materialColor.R == 0 && m.materialColor.G == 0 && m.materialColor.B == 0)
		{	doMesh.Material.Color = new Color(.5f, .5f, .5f, 1);
		}
		if (m.textureFileName != null)	doMesh.Material.Texture = new cTexture(wndDev, Path.Combine(meshDirectory, m.textureFileName), true);
	}
	// ** Cfg vertices
	doMesh.Vertices = new Vertex[vertexList.Length];
	for (int n = 0; n < vertexList.Length; n++)
	{	doMesh.Vertices[n].Position = vertexList[n]; doMesh.Vertices[n].Normal = a_vNorms[n]; // Pos y normal
		if (meshTextureCoords != null)	doMesh.Vertices[n].Texcoord = meshTextureCoords[n]; // Texcoord (opcio)
	}
	doMesh.VertexCant = vertexList.Length;
	doMesh.VertexLayout = (meshTextureCoords != null ? c3DModel.eVertexLayout.PositionNormalTexcoord : c3DModel.eVertexLayout.PositionNormal);
	doMesh.Indices = triangleIndiciesList;
}
}
}