using UnityEngine;

namespace Utilities {
    public class Particles {
        public static void SetParticleEmission(ParticleSystem particle, float targetEmission) {
            var particleEmission = particle.emission;
            particleEmission.rateOverTime = new ParticleSystem.MinMaxCurve(targetEmission);
        }
        
        public static void SetParticleEmission(ParticleSystem[] particles, float targetEmission) {
            foreach (ParticleSystem particle in particles) {
                var particleEmission = particle.emission;
                particleEmission.rateOverTime = new ParticleSystem.MinMaxCurve(targetEmission);
            }
            
        }

        public static void SetStartLifeTime(ParticleSystem particle, float startLifeTime) {
            var particleMain = particle.main;
            particleMain.startLifetime = startLifeTime;
        }

        public static void MultiplyRemainingLifeTime(ParticleSystem particleSystem, float multiplier) {
            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[particleSystem.particleCount];
            particleSystem.GetParticles(particles);
            
            for (int i = 0; i < particles.Length; i++) {
                particles[i].remainingLifetime *= multiplier;
            }
            particleSystem.SetParticles(particles, particles.Length);
        }
        
        public static void SetRemainingLifeTime(ParticleSystem particleSystem, float remainingLifeTime) {
            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[particleSystem.particleCount];
            particleSystem.GetParticles(particles);
            
            for (int i = 0; i < particles.Length; i++) {
                particles[i].remainingLifetime = remainingLifeTime;
            }
            particleSystem.SetParticles(particles, particles.Length);
        }
    }
}