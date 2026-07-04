using UnityEngine;

namespace Wizard;

public class MyPageCustomBGMasterData
{
	public string Id { get; private set; }

	private string NameTextId { get; set; }

	public string Name => Data.Master.GetMyPageBGText(NameTextId);

	public float PositionX { get; private set; }

	public float PositionY { get; private set; }

	public float PositionX2 { get; private set; }

	public float PositionY2 { get; private set; }

	public float Scale { get; private set; }

	public float Scale2 { get; private set; }

	public bool IsFrontEffectAttachCharacter { get; private set; }

	public bool IsBackEffectAttachCharacter { get; private set; }

	public float SpineCameraSize { get; private set; }

	public float ShaderAlphaBorder { get; private set; }

	public float ShaderAlphaDevide { get; private set; }

	public bool IsBGCardShader { get; private set; }

	public MyPageCustomBGMasterData(string[] columns)
	{
		int num = 0;
		Id = columns[num++];
		NameTextId = columns[num++];
		PositionX = float.Parse(columns[num++]);
		PositionY = float.Parse(columns[num++]);
		PositionX2 = float.Parse(columns[num++]);
		PositionY2 = float.Parse(columns[num++]);
		Scale = float.Parse(columns[num++]);
		Scale2 = float.Parse(columns[num++]);
		IsFrontEffectAttachCharacter = int.Parse(columns[num++]) != 0;
		IsBackEffectAttachCharacter = int.Parse(columns[num++]) != 0;
		SpineCameraSize = float.Parse(columns[num++]);
		ShaderAlphaBorder = float.Parse(columns[num++]);
		ShaderAlphaDevide = float.Parse(columns[num++]);
		IsBGCardShader = int.Parse(columns[num++]) != 0;
	}
}
