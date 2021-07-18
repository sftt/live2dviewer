using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

[Serializable]
public class Live2DPartConfig
{
	public string name;
	public int index;
	public float opacity;
}

[Serializable]
public class Live2DModelConfig
{
	public string name;

	public string path;
	public string mocFile;
	public string[] textureFiles;
	public string[] motionFiles;
	public string[] expressionFiles;
	public string poseFile;

	public Live2DPartConfig[] parts;

	public int currentMotionIndex;
	public int currentExpressionIndex;
}

[Serializable]
public class Live2DViewerConfig
{
	public string rootFolder;
	public Live2DModelConfig[] models;
	public int currentModelIndex;
	public string backgroundTexturePath;
	public bool loopMotion = false;

	public Live2DModelConfig currentModel {
		get { return models[currentModelIndex]; } 
	}

	public void NextModel() {
		currentModelIndex++;
		if (currentModelIndex >= models.Length) {
			currentModelIndex = 0;
		}
	}
	public void PrevModel() {
		currentModelIndex--;
		if (currentModelIndex < 0) {
			currentModelIndex = models.Length - 1;
		}
	}

	public void ScanFolder(string path) {
		var modelList = new List<Live2DModelConfig>();

		foreach (string file in Directory.GetDirectories(path)) {
			TryScanModelFolder(file, modelList);
		}

		if (modelList.Count == 0) {
			TryScanModelFolder(path, modelList);
		}

		models = modelList.ToArray();
		rootFolder = path;
		currentModelIndex = 0;
	}

	private void TryScanModelFolder(string path, List<Live2DModelConfig> modelList) {
		var mocFiles = Directory.GetFiles(path, "*.model3.json", SearchOption.AllDirectories);
		if (mocFiles.Length == 0) {
			mocFiles = Directory.GetFiles(path, "*.moc3", SearchOption.AllDirectories); // also search moc3 files
			if (mocFiles.Length == 0)
				return;
		} else {
			foreach (var mocFile in mocFiles) {
				var mocConfig = new Live2DModelConfig();
				mocConfig.name = Path.GetFileNameWithoutExtension(mocFile.Replace(".json", ""));
				mocConfig.path = path;
				mocConfig.mocFile = mocFile;
				modelList.Add(mocConfig);
			}
			return;
		}
		var config = new Live2DModelConfig();

		config.name = Path.GetFileName(path);
		if (String.IsNullOrEmpty(config.name)) {
			config.name = Path.GetFileName(Path.GetDirectoryName(path));
		}
		config.path = path;
		config.mocFile = mocFiles[0];
		config.name = Path.GetFileNameWithoutExtension(config.mocFile);
		var basename = Path.GetFileNameWithoutExtension(config.mocFile.Replace(".json",""));

		foreach (string textureDir in Directory.GetDirectories(path, basename + ".*")) {
			var textures = Directory.GetFiles(textureDir, "*.png");
			if (textures.Length > 0) {
				Array.Sort(textures);
				config.textureFiles = textures;
				break;
			}
		}

		config.motionFiles = Directory.GetFiles(path, "*.motion3.json", SearchOption.AllDirectories);
		if(config.motionFiles.Length == 0)
			config.motionFiles = Directory.GetFiles(path, "*.mtn.bytes", SearchOption.AllDirectories);

		config.expressionFiles = Directory.GetFiles(path, "*.exp.json", SearchOption.AllDirectories);
		var poseFiles = Directory.GetFiles(path, "*.pose.json", SearchOption.AllDirectories);
		if (poseFiles.Length > 0) config.poseFile = poseFiles[0];

		modelList.Add(config);
	}
}

public enum Live2DViewerConfigChangeType {
	RootFolder,
	Model,
	Background,
	LoopMotion,
	Motion,
	Expression,
	Parts
}

[System.Serializable]
public class Live2DViewerConfigEvent : UnityEvent<Live2DViewerConfig, Live2DViewerConfigChangeType>
{
}
