using UnityEngine;
using UnityEngine.UI;

public class PlayerEnergy : MonoBehaviour
{
    public int currentEnergy;
    public int maxEnergy = 100;
    public int energyCost = 50;
    public int energyGain = 25;

    public Slider energySlider;

    void Start()
    {
        currentEnergy = maxEnergy;
        energySlider.maxValue = maxEnergy;
        energySlider.value = currentEnergy;
    }

    public int GetEnergy()
    {
        return currentEnergy;
    }
    public void AddEnergy()
    {
        currentEnergy = (currentEnergy + energyGain) <= 100 ? currentEnergy + energyGain : maxEnergy;
        energySlider.value = currentEnergy;
    }

    public void SubtractEnergy()
    {
        currentEnergy = (currentEnergy - energyCost) > 0 ? currentEnergy - energyCost : 0;
        energySlider.value = currentEnergy;
    }

    public void ResetEnergy()
    {
        currentEnergy = maxEnergy;
        energySlider.value = currentEnergy;
    }
}
