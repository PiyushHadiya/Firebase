using System;
using System.Collections;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

using Firebase.Storage;
using Firebase.Extensions;
using Firebase.Database;


public class Firebaseimagechange : MonoBehaviour
{
    public RawImage rawImage;
    public Text secondShow;
    public Text status;

    private string ImageURL;

    private FirebaseStorage firebaseStorage;
    private DatabaseReference databaseReference;
    private void Start()
    {
        PlayerImageData.FireBaseImageShow = GettingUrlToRealtimeDatabase;
        
        firebaseStorage = FirebaseStorage.DefaultInstance;
    }
    public void GettingUrlToRealtimeDatabase(string imageID) 
    {
        string randomImageSelect = UnityEngine.Random.Range(1, 10).ToString(); // Generate 1-10 Random Image Name

        databaseReference = PlayerImageData.firebaseDatabase.RootReference.Child("Company/ImageContainer").Child(imageID); // Make a path of Image

        databaseReference.GetValueAsync().ContinueWith(task =>
        {
            if (!task.IsFaulted && !task.IsCanceled)
            {
                DataSnapshot snapshot = task.Result;
                ImageURL = snapshot.Value.ToString(); // Collecting Image URL
                PrepareDownloadLink(ImageURL); // Prepare Download link of Image in Firebase Storage 
            }
            else
            {
                Debug.LogError("Download Link Not Generated !");
            }
        });
    }
    private void PrepareDownloadLink(string ImageURL)
    {

        var d = firebaseStorage.GetReferenceFromUrl(ImageURL);

        d.GetDownloadUrlAsync().ContinueWithOnMainThread(task =>
        {
            if (!task.IsFaulted && !task.IsCanceled)
            {
                // covert task result into string
                string dowloadLink = Convert.ToString(task.Result);

                status.text = "Download link Generated!";

                // Go to download Image
                StartCoroutine(DownloadImage(dowloadLink));
            }
        });
    }

    private IEnumerator DownloadImage(string downloadUrl)
    {
        status.text = "Download Start!";
        UnityWebRequest unityWebRequest = UnityWebRequestTexture.GetTexture(downloadUrl);

        // wait for request to downloadUrl
        yield return unityWebRequest.SendWebRequest();

        if (unityWebRequest.result == UnityWebRequest.Result.ConnectionError || unityWebRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"Download is Failed of {unityWebRequest.result}");
            status.text = "Download Error!";
        }
        else
        {
            // Download Image Apply to rawImage texture
            rawImage.texture = ((DownloadHandlerTexture)unityWebRequest.downloadHandler).texture;

            status.text = "Download Complete!";
        }
    }
}

