using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Range(0f, 1f)] public float time;
    public float fullDayLength; // 전체적인 하루의 길이
    public float startTime = 0.4f; // 0.5일 때 정오(12시)

    private float timeRate;
    public Vector3 noon; // 정오 => vector 90 0 0 (Sun 회전)

    [Header("# Sun")] 
    public Light sun;
    public Gradient sunColor;
    public AnimationCurve sunIntensity; 
    
    [Header("# Moon")] 
    public Light moon;
    public Gradient moonColor;
    public AnimationCurve moonIntensity;
    
    [Header("# Other Lighting")] // 라이트 세팅 값 조절
    public AnimationCurve lightingIntensityMultiplier;  
    public AnimationCurve reflectionIntensityMultiplier; // 빛 반사


    private void Start()
    {
        timeRate = 1.0f / fullDayLength; // fullDayLength가 30이면 하루는 30초
        time = startTime;
    }

    private void Update()
    {
        // 시간 업데이트
        time = (time + timeRate * Time.deltaTime) % 1.0f;

        UpdateLighting(sun, sunColor, sunIntensity);
        UpdateLighting(moon, moonColor, moonIntensity);

        RenderSettings.ambientIntensity = lightingIntensityMultiplier.Evaluate(time);
        RenderSettings.reflectionIntensity = reflectionIntensityMultiplier.Evaluate(time);
    }

    private void UpdateLighting(Light lightSource, Gradient gradient, AnimationCurve intensityCurve)
    {
        // 보간되는 값을 받아옴
        float intensity = intensityCurve.Evaluate(time);

        // 라이트의 각도 변환 (시간에 다른 해나 달의 변화)
        // time이 0.5f이면 정오인 시간 -> 각도 90도가 나와야하지만 1f는 360도이기 때문에 0.5를하면 180도가나옴 -> 그래서 0.25f
        // moon일 떄는 정 반대니깐 0.25f에서 0.5f를 더한 0.75f
        lightSource.transform.eulerAngles = noon * ((time - (lightSource == sun ? 0.25f : 0.75f)) * 4f); 
        lightSource.color = gradient.Evaluate(time); // 
        lightSource.intensity = intensity;

        GameObject go = lightSource.gameObject; // 해와 달은 굳이 두개를 띄울 필요가 없기 때문에 하나만 활성화되게 함
        if (lightSource.intensity == 0 && go.activeInHierarchy) 
        {
            go.SetActive(false);
        } 
        else if (lightSource.intensity > 0 && !go.activeInHierarchy)
        {
            go.SetActive(true);
        }            
    }
}
