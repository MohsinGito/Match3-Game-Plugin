using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Security.Cryptography;

public static class Helper
{

    #region Main Camera Refrencing

    private static Camera mainCamera;
    public static Camera MainCamera
    {
        get
        {
            if (mainCamera == null)
                mainCamera = Camera.main;
            return mainCamera;
        }
    }

    #endregion

    #region Getting Await Time To Reduce Garbage Allocation

    private static readonly Dictionary<float, WaitForSeconds> waitDict
         = new Dictionary<float, WaitForSeconds>();
    public static WaitForSeconds WaitFor(float time)
    {
        if (waitDict.TryGetValue(time, out var await)) return await;

        waitDict[time] = new WaitForSeconds(time);
        return waitDict[time];
    }

    #endregion

    #region Checking If Cursor Is Over Any UI

    public static PointerEventData eventData;
    public static List<RaycastResult> results;

    public static bool IsCursorOverUI()
    {
        eventData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }

    #endregion

    #region Getting World Position With Respect To UI Element

    public static Vector2 UIToWorld(RectTransform rectTransform)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, 
            rectTransform.position, MainCamera, out var worldPos);
        return worldPos;
    }

    public static Vector2 WorldToUI(RectTransform canvas, Vector3 position)
    {
        Vector2 temp = MainCamera.WorldToViewportPoint(position);
        temp.x *= canvas.sizeDelta.x;
        temp.y *= canvas.sizeDelta.y;
        temp.x -= canvas.sizeDelta.x * canvas.pivot.x;
        temp.y -= canvas.sizeDelta.y * canvas.pivot.y;

        return temp;
    }

    #endregion

    #region Deleting All Childrens Of Any Transform

    public static void ClearChildren(this Transform itemTransform)
    {
        foreach(Transform child in itemTransform)
        {
            Object.Destroy(child.gameObject);
        }
    }

    #endregion

    #region Make Object Look Towards Other Object 

    public static void LookRotation(this Transform source, Transform target, float lookSpeed)
    {
        source.rotation = Quaternion.Slerp(source.rotation, Quaternion.LookRotation
            (target.position - source.position, Vector3.up), Time.deltaTime * lookSpeed);
    }

    public static void LookRotationX(this Transform source, Transform target, float lookSpeed)
    {
        Vector3 originalRotation = source.eulerAngles;
        source.rotation = Quaternion.Slerp(source.rotation, Quaternion.LookRotation
            (target.position - source.position, Vector3.up), Time.deltaTime * lookSpeed);
        source.eulerAngles = new Vector3(source.eulerAngles.x, originalRotation.y, originalRotation.z);
    }

    public static void LookRotationY(this Transform source, Transform target, float lookSpeed)
    {
        Vector3 originalRotation = source.eulerAngles;
        source.rotation = Quaternion.Slerp(source.rotation, Quaternion.LookRotation
            (target.position - source.position, Vector3.up), Time.deltaTime * lookSpeed);
        source.eulerAngles = new Vector3(originalRotation.x, source.eulerAngles.y, originalRotation.z);
    }

    public static void LookRotationZ(this Transform source, Transform target, float lookSpeed)
    {
        Vector3 originalRotation = source.eulerAngles;
        source.rotation = Quaternion.Slerp(source.rotation, Quaternion.LookRotation
            (target.position - source.position, Vector3.up), Time.deltaTime * lookSpeed);
        source.eulerAngles = new Vector3(originalRotation.x, originalRotation.y, source.eulerAngles.z);
    }

    #endregion

    #region Make Object Rotate Towards Other Object

    public static void RotateTowardsTarget(this Transform source, Transform target, float speed)
    {
        Vector3 newDirection = Vector3.RotateTowards(source.forward,
            target.position - source.position, speed * Time.deltaTime, 0.0f);
        source.rotation = Quaternion.LookRotation(newDirection);
    }

    public static void RotateTowardsTargetX(this Transform source, Transform target, float speed)
    {
        Vector3 originalRotation = source.eulerAngles;
        Vector3 newDirection = Vector3.RotateTowards(source.forward,
            target.position - source.position, speed * Time.deltaTime, 0.0f);
        source.rotation = Quaternion.LookRotation(newDirection);
        source.eulerAngles = new Vector3(source.eulerAngles.x, originalRotation.y, originalRotation.z);
    }

    public static void RotateTowardsTargetY(this Transform source, Transform target, float speed)
    {
        Vector3 originalRotation = source.eulerAngles;
        Vector3 newDirection = Vector3.RotateTowards(source.forward,
            target.position - source.position, speed * Time.deltaTime, 0.0f);
        source.rotation = Quaternion.LookRotation(newDirection);
        source.eulerAngles = new Vector3(source.eulerAngles.x, originalRotation.y, originalRotation.z);
    }

    public static void RotateTowardsTargetZ(this Transform source, Transform target, float speed)
    {
        Vector3 originalRotation = source.eulerAngles;
        Vector3 newDirection = Vector3.RotateTowards(source.forward,
            target.position - source.position, speed * Time.deltaTime, 0.0f);
        source.rotation = Quaternion.LookRotation(newDirection);
        source.eulerAngles = new Vector3(source.eulerAngles.x, originalRotation.y, originalRotation.z);
    }

    #endregion

    #region Check If Object Looking Towards Other

    public static bool IsLooking(this Transform source, Transform target)
    {
        return !(Vector3.Dot(source.position, target.position) < 0.7f);
    }

    public static string CharToString(this List<char> list)
    {
        string strToReturn = "";
        foreach (char item in list)
        {
            strToReturn += item.ToString();
        }
        return strToReturn;
    }

    #endregion

    #region Web Requesting Essentials

    public static string ComputeSha256Hash(string rawData)
    {
        /* -- CREATING A SHA255 -- */
        using (SHA256 sha256Hash = SHA256.Create())
        {
            /* --  -- */ // ComputeHash - returns byte array  
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

            /* -- CONVERTING BYTE ARRAY TO A STRING -- */
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }

    //public static long GetUnixTimestamp()
    //{
    //    /* -- CONVERTING E.G => "12/05/2022 18:39:27" TO "1652351967" -- */
    //    return ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
    //}

    //public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
    //{
    //    /* -- CONVERTING E.G => "1652351967" TO "12/05/2022 18:39:27" -- */
    //    return new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc).
    //            AddSeconds(unixTimeStamp).ToLocalTime();
    //}

    public static bool CheckUserPlayed10Minutes(this int playedTime)
    {
        /* -- GETTING STARTUP IN SECONDS AND CONVERTING IT TO MINUTES -- */
        return (Time.realtimeSinceStartup / 60) > 10;
    }

    #endregion

    #region Strings Operation

    public static string GetConcatination(string str)
    {
        string strToReturn = "";
        char[] charArr = str.ToCharArray();

        for(int i = 0; i < charArr.Length; i++)
        {
            if (charArr[i].Equals(' '))
                strToReturn += "_";
            else
                strToReturn += charArr[i];
        }

        return strToReturn;
    }

    #endregion

}

public class Generics<T>
{

    #region Randomizing List

    public static List<T> Randomize(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
        return list;
    }

    public static List<T> CirculateList(List<T> list)
    {
        T temp = list[0];
        for (int i = 1; i < list.Count; i++)
        {
            list[i - 1] = list[i];
        }
        list[list.Count - 1] = temp;
        return list;
    }

    public static T[,] ReverseArray(T[,] theArray)
    {
        for (int rowIndex = 0;
             rowIndex <= (theArray.GetUpperBound(0)); rowIndex++)
        {
            for (int colIndex = 0;
                 colIndex <= (theArray.GetUpperBound(1) / 2); colIndex++)
            {
                T tempHolder = theArray[rowIndex, colIndex];
                theArray[rowIndex, colIndex] =
                  theArray[rowIndex, theArray.GetUpperBound(1) - colIndex];
                theArray[rowIndex, theArray.GetUpperBound(1) - colIndex] =
                  tempHolder;
            }
        }

        return theArray;
    }

    #endregion

}