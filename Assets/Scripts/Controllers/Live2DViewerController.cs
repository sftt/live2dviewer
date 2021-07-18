using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.IO;
using Live2D.Cubism.Core;
using Live2D.Cubism.Framework.Json;

public class Live2DViewerController : MonoBehaviour
{
	public Live2DViewerConfig config;
	public BackgroundComponent backgroundComponent;
	public CubismModel3Json model3json;
	public CubismModel3Json modeljson;
	public CubismModel model3;
	public CubismModel model;

	public Text indicatorTitle;
	public Text indicatorBody;

	public GameObject settingsPrefab;

	public bool newModel(string name) {
		if (model != null && model.name == name) {
			model.gameObject.SetActive(true);
			var m = model;
			model = model3;
			model3 = m;
			if (model != null) {
				model.gameObject.SetActive(false);
			}
			var mj = modeljson;
			modeljson = model3json;
			model3json = mj;
			return false;
		} else if (model != null) {
			Destroy(model.gameObject);
			//Destroy(model);
			model = null;
			foreach (var texture in modeljson.Textures) {
				Destroy(texture);
			}
			modeljson = null;
		}
		return true;
	}

	public void loadModel(CubismModel newModel, string name, CubismModel3Json newModel3Json = null) {
		if (newModel != null) {
			//newModel.gameObject.SetActive(false);
			newModel.name = name;
			model = model3;
			model3 = newModel;
			if (model != null) {
				model.gameObject.SetActive(false);
			}
			modeljson = model3json;
			model3json = newModel3Json;
		}
	}

	public void OnConfigChanged(Live2DViewerConfig c, Live2DViewerConfigChangeType t) {
		config = c;
		switch(t) {
			case Live2DViewerConfigChangeType.RootFolder:
			case Live2DViewerConfigChangeType.Model:
				if (config.models.Length > 0) {
					var current = config.currentModel;
					if (newModel(current.name)) {
						if (current.mocFile.EndsWith(".model3.json")) {
							var m3j = CubismModel3Json.LoadAtPath(current.mocFile);
							if (m3j != null) {
								loadModel(m3j.ToModel(), current.name, m3j);
							}
						} else {
							var moc = CubismMoc.CreateFrom(File.ReadAllBytes(current.mocFile));
							if (moc != null) {
								loadModel(CubismModel.InstantiateFrom(moc), current.name);
							}
						}
					}
					//_motionController = model3.GetComponent<CubismMotionController>();
				}
				UpdateIndicator();
				break;/*
					modelComponent.LoadFromFiles(current.mocFile, current.textureFiles, current.poseFile);
					if (current.parts == null || current.parts.Length == 0) {
						current.parts = modelComponent.LoadParts();
					} else {
						modelComponent.SetParts(current.parts);
					}

					motionsComponent.LoadFromFile(current.motionFiles);
				} else {
					modelComponent.ReleaseModel();
					motionsComponent.ReleaseMotions();
				}
				expComponent.ReleaseExpression();
				cameraResetAction.Perform();
				UpdateIndicator();
				break;
			case Live2DViewerConfigChangeType.Motion:
				motionsComponent.PlayMotion(config.currentModel.currentMotionIndex);
				break;
			case Live2DViewerConfigChangeType.Expression:
				expComponent.LoadFromFile(config.currentModel.expressionFiles[config.currentModel.currentExpressionIndex]);
				break;
			case Live2DViewerConfigChangeType.LoopMotion:
				motionsComponent.loop = config.loopMotion;
				break;
			case Live2DViewerConfigChangeType.Parts:
				modelComponent.SetParts(config.currentModel.parts);
				break;*/
			case Live2DViewerConfigChangeType.Background:
				backgroundComponent.LoadFromFile(config.backgroundTexturePath);
				break;
		}
	}

	public void ShowConfigPanel() {
		var configController = Instantiate<GameObject>(settingsPrefab, transform.parent.transform, false).GetComponent<ConfigController>();
		configController.config = config;
		configController.Render();
		configController.onConfigChanged.AddListener(OnConfigChanged);
	}

	public void PrevModel() {
		config.PrevModel();
		OnConfigChanged(config, Live2DViewerConfigChangeType.Model);
	}

	public void NextModel() {
		config.NextModel();
		OnConfigChanged(config, Live2DViewerConfigChangeType.Model);
	}

	void UpdateIndicator() {
		if (config.models.Length > 0) {
			var current = config.currentModel;
			indicatorTitle.text = current.name;
			indicatorBody.text = String.Format("{0}/{1}", config.currentModelIndex + 1, config.models.Length);
		} else {
			indicatorTitle.text = "未加载";
			indicatorBody.text = "";
		}
	}

	void Update() {
		if (Input.GetKeyUp(KeyCode.LeftArrow)) {
			PrevModel();
		}
		if (Input.GetKeyUp(KeyCode.RightArrow)) {
			NextModel();
		}
		if (Input.GetKeyUp(KeyCode.Space)) {
			//motionsComponent.NextMotion();
		}
	}
}
