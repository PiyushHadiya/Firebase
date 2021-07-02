using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Firebase.Database;
using Firebase.Storage;
using Firebase.Extensions;
using System.Threading.Tasks;
using System.Threading;
using System;
using UnityEngine.UI;

public class PlayerImageData : MonoBehaviour
{
    public static Action<string> FireBaseImageShow;

    public static FirebaseDatabase firebaseDatabase;
    private FirebaseStorage firebaseStorage;
    private DatabaseReference databaseReference;

    public Text countDown;
    
    public string PlayerId;
    private float Second = 10;
    private int ImageCount;
    private bool IsCountDownStart = false;

    public List<int> ShowedImageList = new List<int>();
    public List<int> UnshowedImageList = new List<int>();

    private void Awake()
    {
        PlayerName();
        OnStart();
    }
    private void PlayerName()
    {
        PlayerId = PlayerPrefs.GetString("localPlayerId", "");

        if (PlayerId.Length <= 0)
        {
            PlayerId = "Geust" + UnityEngine.Random.Range(2020, 4040);
            PlayerPrefs.SetString("localPlayerId", PlayerId);
        }
    }
    private void Update()
    {
    
        if (IsCountDownStart)
        {
            Second -= Time.deltaTime;
            countDown.text = (Mathf.RoundToInt(Second)).ToString();

            if (Second <= 0f)
            {
                if (UnshowedImageList.Count > 0)
                {
                    RandomImageShow();
                    Second = 1f;
                }
                else
                {
                    OnStart();
                }
            }
        }
    }


    private void OnStart()
    {
        ShowedImageList.Clear();
        IsCountDownStart = false; 
        Second = 10f;

        firebaseDatabase = FirebaseDatabase.GetInstance("https://fir-two-1aeaa-default-rtdb.firebaseio.com");
        firebaseStorage = FirebaseStorage.DefaultInstance;

        databaseReference = firebaseDatabase.RootReference;
        databaseReference.Child($"Users/{PlayerId}/PlayerName").SetValueAsync(PlayerId);

        new Thread(GetTotalImages).Start();
    }
    private void GetTotalImages()
    {
        databaseReference = firebaseDatabase.RootReference.Child("Company/TotalImage");
        databaseReference.GetValueAsync().ContinueWith((task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot dataSnapshot = task.Result;
                ImageCount = int.Parse(dataSnapshot.Value.ToString());

                new Thread(this.GetShownImageList).Start();
                new Thread(this.GetUnshownImageList).Start();
            }
            else
            {
                ImageCount = 0;
            }

        }));
    }

    private void GetUnshownImageList()
    {
        for (int i = 0; i < ImageCount; i++)
        {
            UnshowedImageList.Add(i);
        }
    }

    private void GetShownImageList()
    {
        databaseReference = firebaseDatabase.RootReference.Child($"Users/{PlayerId}/ImageShowen");

        for (int i = 0; i < ImageCount; i++)
        {
            databaseReference.Child($"{i}").GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    DataSnapshot dataSnapshot = task.Result;

                    int a = int.Parse(dataSnapshot.Value.ToString());
                    ShowedImageList.Add(a);
                }
            });
        }

        Thread.Sleep(1000);

        if (ShowedImageList.Count != ImageCount)
            new Thread(UnshowedImagesSorting).Start();
        else
            RemoveUserImageData();
    }

    private void RemoveUserImageData()
    {
        databaseReference = firebaseDatabase.RootReference.Child($"Users/{PlayerId}/ImageShowen");
        databaseReference.RemoveValueAsync();
        IsCountDownStart = true;
    }

    private void UnshowedImagesSorting()
    {
        Thread.Sleep(1000);
        IEnumerable<int> uncommon = ShowedImageList.Intersect(UnshowedImageList).ToList();
        UnshowedImageList.RemoveAll(x => uncommon.Contains(x));
        IsCountDownStart = true;
    }


    private void RandomImageShow()
    {
        if (UnshowedImageList.Count >= 0)
        {
            int imageID = UnityEngine.Random.Range(0, UnshowedImageList.Count);
            int Value = UnshowedImageList[imageID];

            UnshowedImageList.Remove(Value);

            databaseReference = firebaseDatabase.RootReference.Child($"Users/{PlayerId}/ImageShowen");
            databaseReference.Child(Value.ToString()).SetValueAsync(Value);

            FireBaseImageShow(Value.ToString());
        }
        else
            RemoveUserImageData();
    }
}
