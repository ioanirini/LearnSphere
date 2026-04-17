using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class MoodleAPI : MonoBehaviour
{
    private string apiURL = "https://your-moodle-site/webservice/rest/server.php";
    private string token = "your-moodle-api-token";

    public void PostGrade(int courseId, int itemId, int userId, float gradeValue)
    {
        StartCoroutine(PostGradeCoroutine(courseId, itemId, userId, gradeValue));
    }

    IEnumerator PostGradeCoroutine(int courseId, int itemId, int userId, float gradeValue)
    {
        // Build POST form
        WWWForm form = new WWWForm();
        form.AddField("wstoken", token);
        form.AddField("wsfunction", "core_grades_update_grades");
        form.AddField("moodlewsrestformat", "json");

        // Add course and user information
        form.AddField("grades[0][courseid]", courseId.ToString());
        form.AddField("grades[0][itemid]", itemId.ToString());
        form.AddField("grades[0][userid]", userId.ToString());
        form.AddField("grades[0][gradevalue]", gradeValue.ToString());

        UnityWebRequest request = UnityWebRequest.Post(apiURL, form);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error: " + request.error);
        }
        else
        {
            Debug.Log("Grade successfully posted: " + request.downloadHandler.text);
        }
    }
}