using System;
using System.Collections.Generic;
using UnityEngine;

public class MotionUtils
{
	public enum EaseType
	{
		linear,
		easeInSine,
		easeInQuad,
		easeInCubic,
		easeInQuart,
		easeInQuint,
		easeInExpo,
		easeInBack,
		easeInBounce,
		easeInElastic,
		easeOutSine,
		easeOutQuad,
		easeOutCubic,
		easeOutQuart,
		easeOutQuint,
		easeOutExpo,
		easeOutBack,
		easeOutBounce,
		easeOutElastic,
		easeInOutSine,
		easeInOutQuad,
		easeInOutCubic,
		easeInOutQuart,
		easeInOutQuint,
		easeInOutExpo,
		easeInOutBack,
		easeInOutBounce,
		easeInOutElastic
	}

	public static float GetEase(float t, EaseType e)
	{
		float num = 1f - t;
		switch (e)
		{
		case EaseType.linear:
			return t;
		case EaseType.easeInSine:
			return 1f - Mathf.Cos(t * (float)Math.PI / 2f);
		case EaseType.easeInQuad:
			return t * t;
		case EaseType.easeInCubic:
			return t * t * t;
		case EaseType.easeInQuart:
			return t * t * t * t;
		case EaseType.easeInQuint:
			return t * t * t * t * t;
		case EaseType.easeInExpo:
			return Mathf.Pow(2f, 10f * (t - 1f));
		case EaseType.easeInBack:
			return t * t * (2.70158f * t - 1.70158f);
		case EaseType.easeInBounce:
			if (num < 0.36363637f)
			{
				return 1f - 7.5625f * num * num;
			}
			if (num < 0.72727275f)
			{
				return 1f - (7.5625f * (num - 0.54545456f) * (num - 0.54545456f) + 0.75f);
			}
			if (num < 0.90909094f)
			{
				return 1f - (7.5625f * (num - 0.8181818f) * (num - 0.8181818f) + 0.9375f);
			}
			return 1f - (7.5625f * (num - 21f / 22f) * (num - 21f / 22f) + 63f / 64f);
		case EaseType.easeInElastic:
		{
			float num2 = 0.3f;
			if (t == 0f)
			{
				return 0f;
			}
			if (t == 1f)
			{
				return 1f;
			}
			float num3 = num2 / ((float)Math.PI * 2f) * Mathf.Asin(1f);
			return 0f - t * Mathf.Pow(2f, 10f * (t -= 1f)) * Mathf.Sin((t - num3) * ((float)Math.PI * 2f) / num2);
		}
		case EaseType.easeOutSine:
			return Mathf.Sin(t * (float)Math.PI / 2f);
		case EaseType.easeOutQuad:
			return 1f - num * num;
		case EaseType.easeOutCubic:
			return 1f - num * num * num;
		case EaseType.easeOutQuart:
			return 1f - num * num * num * num;
		case EaseType.easeOutQuint:
			return 1f - num * num * num * num * num;
		case EaseType.easeOutExpo:
			return 0f - Mathf.Pow(2f, -10f * t) + 1f;
		case EaseType.easeOutBack:
			return 1f - num * num * (2.70158f * num - 1.70158f);
		case EaseType.easeOutBounce:
			if (t < 0.36363637f)
			{
				return 7.5625f * t * t;
			}
			if (t < 0.72727275f)
			{
				return 7.5625f * (t - 0.54545456f) * (t - 0.54545456f) + 0.75f;
			}
			if (t < 0.90909094f)
			{
				return 7.5625f * (t - 0.8181818f) * (t - 0.8181818f) + 0.9375f;
			}
			return 7.5625f * (t - 21f / 22f) * (t - 21f / 22f) + 63f / 64f;
		case EaseType.easeOutElastic:
		{
			float num2 = 0.3f;
			if (t == 0f)
			{
				return 0f;
			}
			if (t == 1f)
			{
				return 1f;
			}
			float num3 = num2 / ((float)Math.PI * 2f) * Mathf.Asin(1f);
			return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t - num3) * ((float)Math.PI * 2f) / num2) + 1f;
		}
		case EaseType.easeInOutSine:
			return (1f - Mathf.Cos(t * (float)Math.PI)) * 0.5f;
		case EaseType.easeInOutQuad:
			return (t < 0.5f) ? (t * t * 2f) : (1f - num * num * 2f);
		case EaseType.easeInOutCubic:
			return (t < 0.5f) ? (t * t * t * 2f) : (1f - num * num * num * 2f);
		case EaseType.easeInOutQuart:
			return (t < 0.5f) ? (t * t * t * t * 2f) : (1f - num * num * num * num * 2f);
		case EaseType.easeInOutQuint:
			return (t < 0.5f) ? (t * t * t * t * t * 2f) : (1f - num * num * num * num * num * 2f);
		case EaseType.easeInOutExpo:
			return (t < 0.5f) ? Mathf.Pow(2f, 10f * (t * 2f - 1f)) : (0.5f * (0f - Mathf.Pow(2f, -10f * (t * 2f - 1f)) + 2f));
		case EaseType.easeInOutBack:
			return (t < 0.5f) ? (0.5f * (t * 2f) * (t * 2f) * (2.525f * t * 2f - 1.525f)) : (1f - 0.5f * (num * 2f) * (num * 2f) * (2.525f * num * 2f - 1.525f));
		case EaseType.easeInOutBounce:
			if (t < 0.5f)
			{
				return GetEase(t * 2f, EaseType.easeInBounce) * 0.5f;
			}
			return GetEase(t * 2f - 1f, EaseType.easeOutBounce) * 0.5f + 0.5f;
		case EaseType.easeInOutElastic:
			if (t < 0.5f)
			{
				return GetEase(t * 2f, EaseType.easeInElastic) * 0.5f;
			}
			return GetEase(t * 2f - 1f, EaseType.easeOutElastic) * 0.5f + 0.5f;
		default:
			return t;
		}
	}

	public static void SetLayerAll(GameObject obj, int layer)
	{
		obj.layer = layer;
		for (int i = 0; i < obj.transform.childCount; i++)
		{
			GameObject gameObject = obj.transform.GetChild(i).gameObject;
			if (gameObject.transform.childCount > 0)
			{
				SetLayerAll(gameObject, layer);
			}
			else
			{
				gameObject.layer = layer;
			}
		}
	}

	public static void SetActiveAll(GameObject obj, bool flg)
	{
		obj.SetActive(flg);
		for (int i = 0; i < obj.transform.childCount; i++)
		{
			GameObject gameObject = obj.transform.GetChild(i).gameObject;
			if (gameObject.transform.childCount > 0)
			{
				SetActiveAll(gameObject, flg);
			}
			else
			{
				gameObject.SetActive(flg);
			}
		}
	}

	public static void ChangeParticleSystemColor(GameObject particleSystemRootObject, Color newColor, Action<ParticleSystem> actionOnChildrenParticleSytems = null)
	{
		ParticleSystem[] componentsInChildren = particleSystemRootObject.GetComponentsInChildren<ParticleSystem>();
		foreach (ParticleSystem particleSystem in componentsInChildren)
		{
			ParticleSystem.MainModule main = particleSystem.main;
			main.startColor = newColor;
			ParticleSystem.Particle[] array = new ParticleSystem.Particle[particleSystem.particleCount];
			int particles = particleSystem.GetParticles(array);
			for (int j = 0; j < array.Length; j++)
			{
				array[j].startColor = newColor;
			}
			particleSystem.SetParticles(array, particles);
			actionOnChildrenParticleSytems?.Invoke(particleSystem);
		}
	}

	public static float CalculateFrameRateIndependantDampingConstant(float smoothingAmount, float decayMultiplier)
	{
		return 1f - Mathf.Pow(smoothingAmount, Time.smoothDeltaTime * decayMultiplier);
	}
}
