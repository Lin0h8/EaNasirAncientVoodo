using UnityEngine;
using System.Collections;

public class Teleporter : MonoBehaviour
{
    public DungeonGenerationScript dungeonGenerator;
    public GameObject player;
    public TextInstructions textInstructions;
    public string instructionMessage = "Find the breaker and activate it!";

    private void Start()
    {
        StartCoroutine(TeleportPlayerWhenReady());
    }

    private IEnumerator TeleportPlayerWhenReady()
    {
        while (!dungeonGenerator.isDone)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);

        if (dungeonGenerator.roomObjects.Count > 0 &&
            dungeonGenerator.roomObjects[0].Count > 0)
        {
            CharacterController controller = player.GetComponent<CharacterController>();

            if (controller != null)
            {
                controller.enabled = false;
            }

            GameObject centerTile = dungeonGenerator.roomObjects[0][dungeonGenerator.roomObjects[0].Count / 2];
            Vector3 targetPosition = new Vector3(
                centerTile.transform.GetComponent<Renderer>().bounds.center.x,
                centerTile.transform.position.y + player.transform.GetComponent<Renderer>().bounds.extents.y,
                centerTile.transform.GetComponent<Renderer>().bounds.center.z
            );

            player.transform.position = targetPosition;

            if (controller != null)
            {
                controller.enabled = true;
            }

            Debug.Log($"Player teleported to: {targetPosition}");

            yield return new WaitForSeconds(3f);

            if (textInstructions != null)
            {
                textInstructions.ShowInstructions(instructionMessage);
            }
        }
    }
}