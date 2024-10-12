using MFPS.Audio;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MFPS.Addon.GameResumePro
{
    public class bl_GameResumeProProgress : MonoBehaviour
    {
        [Header("Settings")]
        public float fillBarDuration = 2;
        public float newLevelFadeDuration = 0.3f;

        public AnimationCurve barCompleteAlpha;

        [Header("References")]
        public bl_AudioBank audioBank;
        public Image progressBarBase;
        public Slider progressBarDifference;
        public TextMeshProUGUI gainedXpText;
        public TextMeshProUGUI relativeXpText;
        public TextMeshProUGUI levelNameText;
        public bl_GameResumePro resumeFetcher;
        public CanvasGroup barAlpha;
        public bl_GRLevelBox[] levelBoxes;

        private AudioSource audioSource;
        private int animationState = 0;
        private Action animationCallback;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            if (animationState == -1)
            {
                SkipToFinish();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDisable()
        {
            if (animationState == 1) animationState = -1;//animation canceled before finish
        }

        /// <summary>
        /// 
        /// </summary>
        public void Show(Action callback)
        {
            animationState = 1;
            animationCallback = callback;
            SetupInit();
            StartCoroutine(PlayProgressSequence(callback));
        }

        /// <summary>
        /// 
        /// </summary>
        void SetupInit()
        {
            audioSource = GetComponent<AudioSource>();
            progressBarBase.fillAmount = 0;
#if LM
            // get the current player level info
            var level = bl_LevelManager.Instance.GetLevel(resumeFetcher.GetStat("start-score"));
            var nextLevel = bl_LevelManager.Instance.GetLevelByID(level.LevelID);
            levelBoxes[0].Set(level);
            levelBoxes[1].Set(nextLevel);

            relativeXpText.text = $"{resumeFetcher.GetStat("start-score")} / {nextLevel.ScoreNeeded}";
            levelNameText.text = level.Name.ToUpper();
#endif  
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator PlayProgressSequence(Action callback)
        {
#if LM
            int startScore = resumeFetcher.GetStat("start-score");
            int newScore = startScore + resumeFetcher.GetStat("total-score-gained");

            //the player level with which start the game
            int initialLevel = bl_LevelManager.Instance.GetLevelID(startScore) + 1;
            //the player level after the game finish and apply the new score/xp
            int newLevel = bl_LevelManager.Instance.GetLevelID(newScore) + 1;
            //the levels difference
            int levelsChanged = newLevel - initialLevel + 1;
            gainedXpText.text = $"{startScore} XP";

            //set the initial level percentage complete
            float percentage = progressBarDifference.value = GetRelaviteScorePercentage(startScore);
            progressBarBase.fillAmount = percentage;

            yield return new WaitForSeconds(2);

            int awardScore = 0;
            for (int i = 0; i < levelsChanged; i++)
            {
                int currentLevelID = (initialLevel - 1) + i;
                var currentLevelInfo = bl_LevelManager.Instance.GetLevelByID(currentLevelID);
                var nextLevelInfo = bl_LevelManager.Instance.GetLevelByID(currentLevelID + 1);

                float currentPercentage = 1;
                //the xp that the player will earn for this level
                int toGainXp = nextLevelInfo.GetRelativeScoreNeeded();
                levelNameText.text = currentLevelInfo.Name.ToUpper();

                // if this is the first interaction
                if (i == 0)
                {
                    //deduct the xp that already gain in this level
                    toGainXp -= Mathf.FloorToInt(toGainXp * percentage);
                }
                else
                {
                    // if this is not the first interaction it means the player complete the last level
                    // so reset the progress bar
                    progressBarBase.fillAmount = 0;
                    progressBarDifference.value = 0.002f;
                    percentage = 0;

                    //deduct the remain level score needed
                    toGainXp -= Mathf.FloorToInt(toGainXp * (1 - currentPercentage));

                    audioBank.PlayAudioInSource(audioSource, "new-level");
                    levelBoxes[0].Set(bl_LevelManager.Instance.GetLevelByID(currentLevelID));
                    levelBoxes[1].Set(bl_LevelManager.Instance.GetLevelByID(currentLevelID + 1));

                    yield return new WaitForSeconds(0.7f);
                }

                // if this is the last new level
                if (i == levelsChanged - 1)
                {
                    //calculate the percentage based in the score (instead of 1)
                    currentPercentage = GetRelaviteScorePercentage(newScore);
                }

                float d = 0;
                float p = percentage;
                float duration = fillBarDuration;
                if (currentPercentage > 0) duration *= currentPercentage;

                var ainfo = audioBank.PlayAudioInSource(audioSource, "bar");
                audioSource.pitch = 0.1f + Mathf.Max(0.75f, ainfo.Clip.length / duration);

                int relativeNeededScore = nextLevelInfo.GetRelativeScoreNeeded();
                levelNameText.text = currentLevelInfo.Name.ToUpper();

                while (d < 1)
                {
                    d += Time.deltaTime / duration;
                    p = Mathf.Lerp(percentage, currentPercentage, d);

                    int xp = Mathf.FloorToInt(toGainXp * d);
                    progressBarDifference.value = p;
                    gainedXpText.text = $"AWARD SCORE + {awardScore + xp} XP";
                    relativeXpText.text = $"{xp} / {relativeNeededScore}";
                    yield return null;
                }
                awardScore += toGainXp;
                gainedXpText.text = $"AWARD SCORE + {awardScore} XP";
                relativeXpText.text = $"{Mathf.FloorToInt(relativeNeededScore * currentPercentage)} / {relativeNeededScore}";

                audioSource.Stop();
                yield return StartCoroutine(NewLevelSequence());
            }
            gainedXpText.text = $"AWARD SCORE + {resumeFetcher.GetStat("total-score-gained")} XP";
            animationState = 2;
            callback?.Invoke();
#else
            callback?.Invoke();
            yield break;
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        public void SkipToFinish()
        {
#if LM
            var levelID = bl_LevelManager.Instance.GetRuntimeLevelID();
            var currentScore = bl_LevelManager.Instance.GetRuntimeLocalScore();
            var oldScore = resumeFetcher.GetStat("start-score");
            var oldLevelID = bl_LevelManager.Instance.GetLevel(oldScore);

            levelBoxes[0].Set(bl_LevelManager.Instance.GetLevelByID(levelID));
            levelBoxes[1].Set(bl_LevelManager.Instance.GetLevelByID(levelID + 1));
            barAlpha.alpha = 1;

            var currentPercentage = bl_LevelManager.Instance.GetRelaviteScorePercentage(currentScore);
            progressBarDifference.value = currentPercentage;
            var relativeNeeded = bl_LevelManager.Instance.GetLevelByID(levelID).GetRelativeScoreNeeded();
            relativeXpText.text = $"{Mathf.FloorToInt(relativeNeeded * currentPercentage)} / {relativeNeeded}";
            levelNameText.text = bl_LevelManager.Instance.GetLevelByID(levelID).Name.ToUpper();

            if (levelID == oldLevelID.LevelID)
            {
                currentPercentage = bl_LevelManager.Instance.GetRelaviteScorePercentage(oldScore);
                progressBarBase.fillAmount = currentPercentage;
            }
            else progressBarBase.fillAmount = 0;

            var awardScore = currentScore - oldScore;

            gainedXpText.text = $"AWARD SCORE + {awardScore} XP";
#endif
            animationCallback?.Invoke();
            animationState = 2;
        }

        /// <summary>
        /// use this instead of the level manager function to add compatibility with older versions
        /// </summary>
        /// <param name="score"></param>
        /// <returns></returns>
        public float GetRelaviteScorePercentage(int score)
        {
#if LM
            int scoreLevel = bl_LevelManager.Instance.GetLevelID(score) + 1;
            var levels = bl_LevelManager.Instance.Levels;

            int relativeScore = score;
            int relativeScoreNeeded = levels[scoreLevel + 1].ScoreNeeded;

            relativeScore -= levels[scoreLevel - 1].ScoreNeeded;
            relativeScoreNeeded -= levels[scoreLevel].ScoreNeeded;

            if (relativeScoreNeeded <= 0) return 0;

            return (float)relativeScore / (float)relativeScoreNeeded;
#else
return 0;
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator NewLevelSequence()
        {
            float d = 0;
            float t;
            while (d < 1)
            {
                d += Time.deltaTime / newLevelFadeDuration;
                t = barCompleteAlpha.Evaluate(d);
                barAlpha.alpha = t;
                yield return null;
            }
            yield return new WaitForSeconds(0.2f);

        }
    }
}