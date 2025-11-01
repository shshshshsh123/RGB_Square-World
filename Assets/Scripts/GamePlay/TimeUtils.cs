using UnityEngine;
using System.Collections;

public static class TimeUtils
{
    /// <summary>
    /// 시간 멈추는 코루틴 입니다. (리얼타임기반)
    /// </summary>
    /// <param name="durationRealtime"></param>
    /// <returns></returns>
    public static IEnumerator HitStop(float durationRealtime)
    {
        float original = Time.timeScale;
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(durationRealtime);
        Time.timeScale = original;
    }
}
