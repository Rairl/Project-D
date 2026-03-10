using UnityEngine;

public class RunePuzzleManager : MonoBehaviour
{
    public static RunePuzzleManager Instance;

    public GameObject finalPuzzle;
    public int generatorsActivated = 0;
    public int totalGenerators = 3;

    void Awake()
    {
        Instance = this;
    }

    //All generators Activated
    public void GeneratorCompleted()
    {
        generatorsActivated++;

        if (generatorsActivated >= totalGenerators)
        {
            ActivateFinalPuzzle();
        }
    }

    //Activate Final Puzzle
    void ActivateFinalPuzzle()
    {
        finalPuzzle.SetActive(true);
        Debug.Log("Final Rune Puzzle Activated");
    }
}
