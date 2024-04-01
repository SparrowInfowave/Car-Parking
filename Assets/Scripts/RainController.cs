using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class RainController : MonoBehaviour
{
   [SerializeField] private ParticleSystem rain, rainDropRing;

   private DateTime _time = DateTime.Now;
   private bool _isRainStart = false;

   private readonly int _rainParticleCount = 100;
   private readonly int _rainDropParticle = 125;
   private int _rainStartStopTime = 30;
   private void Start()
   {
      _time = DateTime.Now;
      _rainStartStopTime = Random.Range(25, 35);
      StopRain();
      InvokeRepeating(nameof(CheckRainStart),5f,5f);
   }

   private void CheckRainStart()
   {
      if (!_isRainStart && (DateTime.Now - _time).Seconds > _rainStartStopTime)
      {
         StartRain();
         _isRainStart = true;
         _rainStartStopTime = Random.Range(110, 130);
         _time = DateTime.Now;
         return;
      }
      
      if (_isRainStart && (DateTime.Now - _time).Seconds > _rainStartStopTime)
      {
         StopRain();
         _isRainStart = false;
         _rainStartStopTime = Random.Range(25, 35);
         _time = DateTime.Now;
      }
   }

   private void StartRain()
   {
      var raiEmissionModule = rain.emission;
      raiEmissionModule.rateOverTime = new ParticleSystem.MinMaxCurve(_rainParticleCount);

      var raiDropEmissionModule = rainDropRing.emission;
      raiDropEmissionModule.rateOverTime = new ParticleSystem.MinMaxCurve(_rainDropParticle);
   }
   
   private void StopRain()
   {
      var raiEmissionModule = rain.emission;
      raiEmissionModule.rateOverTime = new ParticleSystem.MinMaxCurve(0);

      var raiDropEmissionModule = rainDropRing.emission;
      raiDropEmissionModule.rateOverTime = new ParticleSystem.MinMaxCurve(0);
   }
}
