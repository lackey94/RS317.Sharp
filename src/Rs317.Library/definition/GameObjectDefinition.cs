public sealed class GameObjectDefinition
{
	public static GameObjectDefinition getDefinition(int objectId)
	{
		for(int c = 0; c < 20; c++)
			if(cache[c].id == objectId)
				return cache[c];

		cacheIndex = (cacheIndex + 1) % 20;
		GameObjectDefinition definition = cache[cacheIndex];
		stream.position = streamOffsets[objectId];
		definition.id = objectId;
		definition.setDefaults();
		definition.loadDefinition(stream);
		return definition;
	}

	public static void load(Archive archive)
	{
		stream = new Buffer(archive.decompressFile("loc.dat"));
		Buffer stream = new Buffer(archive.decompressFile("loc.idx"));
		int objectCount = stream.getUnsignedLEShort();
		streamOffsets = new int[objectCount];
		int offset = 2;
		for(int index = 0; index < objectCount; index++)
		{
			streamOffsets[index] = offset;
			offset += stream.getUnsignedLEShort();
		}

		cache = new GameObjectDefinition[20];
		for(int c = 0; c < 20; c++)
			cache[c] = new GameObjectDefinition();

	}

	public static void nullLoader()
	{
		modelCache = null;
		animatedModelCache = null;
		streamOffsets = null;
		cache = null;
		stream = null;
	}

	public bool unknownAttribute;

	private byte ambient;

	private int translateX;

	public String name;

	private int scaleZ;

	private static Model[] models = new Model[4];

	private byte diffuse;

	public int sizeX;

	private int translateY;

	public int icon;
	private int[] originalModelColors;
	private int scaleX;
	public int configIds;
	private bool rotated;
	public static bool lowMemory;
	private static Buffer stream;
	public int id;
	private static int[] streamOffsets;
	public bool walkable;
	public int mapScene;
	public int childIds[];
	private int _solid;
	public int sizeY;
	public bool adjustToTerrain;
	public bool wall;
	public static Client clientInstance;
	private bool unwalkableSolid;
	public bool solid;
	public int face;
	private bool delayShading;
	private static int cacheIndex;
	private int scaleY;
	private int[] modelIds;
	public int varBitId;
	public int offsetAmplifier;
	private int[] modelTypes;
	public byte description[];
	public bool hasActions;
	public bool castsShadow;
	public static Cache animatedModelCache = new Cache(30);
	public int animationId;
	private static GameObjectDefinition[] cache;
	private int translateZ;
	private int[] modifiedModelColors;
	public static Cache modelCache = new Cache(500);
	public String actions[];

	private GameObjectDefinition()
	{
		id = -1;
	}

	private Model getAnimatedModel(int type, int animationId, int face)
	{
		Model subModel = null;
		long hash;
		if(modelTypes == null)
		{
			if(type != 10)
				return null;
			hash = (id << 6) + face + ((long)(animationId + 1) << 32);
			Model cachedModel = (Model)animatedModelCache.get(hash);
			if(cachedModel != null)
				return cachedModel;
			if(modelIds == null)
				return null;
			bool mirror = rotated ^ (face > 3);
			int modelCount = modelIds.length;
			for(int m = 0; m < modelCount; m++)
			{
				int subModelId = modelIds[m];
				if(mirror)
					subModelId += 0x10000;
				subModel = (Model)modelCache.get(subModelId);
				if(subModel == null)
				{
					subModel = Model.getModel(subModelId & 0xffff);
					if(subModel == null)
						return null;
					if(mirror)
						subModel.mirror();
					modelCache.put(subModel, subModelId);
				}
				if(modelCount > 1)
					models[m] = subModel;
			}

			if(modelCount > 1)
				subModel = new Model(modelCount, models);
		}
		else
		{
			int modelType = -1;
			for(int t = 0; t < modelTypes.length; t++)
			{
				if(modelTypes[t] != type)
					continue;
				modelType = t;
				break;
			}

			if(modelType == -1)
				return null;
			hash = (id << 6) + (modelType << 3) + face + ((long)(animationId + 1) << 32);
			Model model = (Model)animatedModelCache.get(hash);
			if(model != null)
				return model;
			int modelId = modelIds[modelType];
			bool mirror = rotated ^ (face > 3);
			if(mirror)
				modelId += 0x10000;
			subModel = (Model)modelCache.get(modelId);
			if(subModel == null)
			{
				subModel = Model.getModel(modelId & 0xffff);
				if(subModel == null)
					return null;
				if(mirror)
					subModel.mirror();
				modelCache.put(subModel, modelId);
			}
		}
		bool scale;
		scale = scaleX != 128 || scaleY != 128 || scaleZ != 128;
		bool translate;
		translate = translateX != 0 || translateY != 0 || translateZ != 0;
		Model animatedModel = new Model(modifiedModelColors == null, Animation.isNullFrame(animationId),
				face == 0 && animationId == -1 && !scale && !translate, subModel);
		if(animationId != -1)
		{
			animatedModel.createBones();
			animatedModel.applyTransformation(animationId);
			animatedModel.triangleSkin = null;
			animatedModel.vertexSkin = null;
		}
		while(face-- > 0)
			animatedModel.rotate90Degrees();
		if(modifiedModelColors != null)
		{
			for(int c = 0; c < modifiedModelColors.length; c++)
				animatedModel.recolour(modifiedModelColors[c], originalModelColors[c]);

		}
		if(scale)
			animatedModel.scaleT(scaleX, scaleZ, scaleY);
		if(translate)
			animatedModel.translate(translateX, translateY, translateZ);
		animatedModel.applyLighting(64 + ambient, 768 + diffuse * 5, -50, -10, -50, !delayShading);
		if(_solid == 1)
			animatedModel.anInt1654 = animatedModel.modelHeight;
		animatedModelCache.put(animatedModel, hash);
		return animatedModel;
	}

	public GameObjectDefinition getChildDefinition()
	{
		int child = -1;
		if(varBitId != -1)
		{
			VarBit varBit = VarBit.values[varBitId];
			int configId = varBit.configId;
			int lsb = varBit.leastSignificantBit;
			int msb = varBit.mostSignificantBit;
			int bit = Client.BITFIELD_MAX_VALUE[msb - lsb];
			child = clientInstance.interfaceSettings[configId] >> lsb & bit;
		}
		else if(configIds != -1)
			child = clientInstance.interfaceSettings[configIds];
		if(child < 0 || child >= childIds.length || childIds[child] == -1)
			return null;
		else
			return getDefinition(childIds[child]);
	}

	public Model getModelAt(int i, int j, int k, int l, int i1, int j1, int k1)
	{
		Model model = getAnimatedModel(i, k1, j);
		if(model == null)
			return null;
		if(adjustToTerrain || delayShading)
			model = new Model(adjustToTerrain, delayShading, model);
		if(adjustToTerrain)
		{
			int l1 = (k + l + i1 + j1) / 4;
			for(int v = 0; v < model.vertexCount; v++)
			{
				int x = model.verticesX[v];
				int z = model.verticesZ[v];
				int l2 = k + ((l - k) * (x + 64)) / 128;
				int i3 = j1 + ((i1 - j1) * (x + 64)) / 128;
				int j3 = l2 + ((i3 - l2) * (z + 64)) / 128;
				model.verticesY[v] += j3 - l1;
			}

			model.normalise();
		}
		return model;
	}

	private void loadDefinition(Buffer stream)
	{
		int _actions = -1;
		label0: do
		{
			int opcode;
			do
			{
				opcode = stream.getUnsignedByte();
				if(opcode == 0)
					break label0;
				if(opcode == 1)
				{
					int modelCount = stream.getUnsignedByte();
					if(modelCount > 0)
						if(modelIds == null || lowMemory)
						{
							modelTypes = new int[modelCount];
							modelIds = new int[modelCount];
							for(int m = 0; m < modelCount; m++)
							{
								modelIds[m] = stream.getUnsignedLEShort();
								modelTypes[m] = stream.getUnsignedByte();
							}

						}
						else
						{
							stream.position += modelCount * 3;
						}
				}
				else if(opcode == 2)
					name = stream.getString();
				else if(opcode == 3)
					description = stream.readBytes();
				else if(opcode == 5)
				{
					int modelCount = stream.getUnsignedByte();
					if(modelCount > 0)
						if(modelIds == null || lowMemory)
						{
							modelTypes = null;
							modelIds = new int[modelCount];
							for(int m = 0; m < modelCount; m++)
								modelIds[m] = stream.getUnsignedLEShort();

						}
						else
						{
							stream.position += modelCount * 2;
						}
				}
				else if(opcode == 14)
					sizeX = stream.getUnsignedByte();
				else if(opcode == 15)
					sizeY = stream.getUnsignedByte();
				else if(opcode == 17)
					solid = false;
				else if(opcode == 18)
					walkable = false;
				else if(opcode == 19)
				{
					_actions = stream.getUnsignedByte();
					if(_actions == 1)
						hasActions = true;
				}
				else if(opcode == 21)
					adjustToTerrain = true;
				else if(opcode == 22)
					delayShading = true;
				else if(opcode == 23)
					wall = true;
				else if(opcode == 24)
				{
					animationId = stream.getUnsignedLEShort();
					if(animationId == 65535)
						animationId = -1;
				}
				else if(opcode == 28)
					offsetAmplifier = stream.getUnsignedByte();
				else if(opcode == 29)
					ambient = stream.get();
				else if(opcode == 39)
					diffuse = stream.get();
				else if(opcode >= 30 && opcode < 39)
				{
					if(actions == null)
						actions = new String[5];
					actions[opcode - 30] = stream.getString();
					if(actions[opcode - 30].equalsIgnoreCase("hidden"))
						actions[opcode - 30] = null;
				}
				else if(opcode == 40)
				{
					int colourCount = stream.getUnsignedByte();
					modifiedModelColors = new int[colourCount];
					originalModelColors = new int[colourCount];
					for(int c = 0; c < colourCount; c++)
					{
						modifiedModelColors[c] = stream.getUnsignedLEShort();
						originalModelColors[c] = stream.getUnsignedLEShort();
					}

				}
				else if(opcode == 60)
					icon = stream.getUnsignedLEShort();
				else if(opcode == 62)
					rotated = true;
				else if(opcode == 64)
					castsShadow = false;
				else if(opcode == 65)
					scaleX = stream.getUnsignedLEShort();
				else if(opcode == 66)
					scaleY = stream.getUnsignedLEShort();
				else if(opcode == 67)
					scaleZ = stream.getUnsignedLEShort();
				else if(opcode == 68)
					mapScene = stream.getUnsignedLEShort();
				else if(opcode == 69)
					face = stream.getUnsignedByte();
				else if(opcode == 70)
					translateX = stream.getShort();
				else if(opcode == 71)
					translateY = stream.getShort();
				else if(opcode == 72)
					translateZ = stream.getShort();
				else if(opcode == 73)
					unknownAttribute = true;
				else if(opcode == 74)
				{
					unwalkableSolid = true;
				}
				else
				{
					if(opcode != 75)
						continue;
					_solid = stream.getUnsignedByte();
				}
				continue label0;
			} while(opcode != 77);
			varBitId = stream.getUnsignedLEShort();
			if(varBitId == 65535)
				varBitId = -1;
			configIds = stream.getUnsignedLEShort();
			if(configIds == 65535)
				configIds = -1;
			int childCount = stream.getUnsignedByte();
			childIds = new int[childCount + 1];
			for(int c = 0; c <= childCount; c++)
			{
				childIds[c] = stream.getUnsignedLEShort();
				if(childIds[c] == 65535)
					childIds[c] = -1;
			}

		} while(true);
		if(_actions == -1)
		{
			hasActions = modelIds != null && (modelTypes == null || modelTypes[0] == 10);
			if(actions != null)
				hasActions = true;
		}
		if(unwalkableSolid)
		{
			solid = false;
			walkable = false;
		}
		if(_solid == -1)
			_solid = solid ? 1 : 0;
	}

	public bool modelCached()
	{
		if(modelIds == null)
			return true;
		bool cached = true;
		for(int m = 0; m < modelIds.length; m++)
			cached &= Model.isCached(modelIds[m] & 0xffff);
		return cached;
	}

	public bool modelTypeCached(int modelType)
	{
		if(modelTypes == null)
		{
			if(modelIds == null)
				return true;
			if(modelType != 10)
				return true;
			bool cached = true;
			for(int id = 0; id < modelIds.length; id++)
				cached &= Model.isCached(modelIds[id] & 0xffff);

			return cached;
		}
		for(int type = 0; type < modelTypes.length; type++)
			if(modelTypes[type] == modelType)
				return Model.isCached(modelIds[type] & 0xffff);

		return true;
	}

	public void passivelyRequestModels(OnDemandFetcher requester)
	{
		if(modelIds == null)
			return;
		for(int modelId = 0; modelId < modelIds.length; modelId++)
			requester.passiveRequest(modelIds[modelId] & 0xffff, 0);
	}

	private void setDefaults()
	{
		modelIds = null;
		modelTypes = null;
		name = null;
		description = null;
		modifiedModelColors = null;
		originalModelColors = null;
		sizeX = 1;
		sizeY = 1;
		solid = true;
		walkable = true;
		hasActions = false;
		adjustToTerrain = false;
		delayShading = false;
		wall = false;
		animationId = -1;
		offsetAmplifier = 16;
		ambient = 0;
		diffuse = 0;
		actions = null;
		icon = -1;
		mapScene = -1;
		rotated = false;
		castsShadow = true;
		scaleX = 128;
		scaleY = 128;
		scaleZ = 128;
		face = 0;
		translateX = 0;
		translateY = 0;
		translateZ = 0;
		unknownAttribute = false;
		unwalkableSolid = false;
		_solid = -1;
		varBitId = -1;
		configIds = -1;
		childIds = null;
	}
}