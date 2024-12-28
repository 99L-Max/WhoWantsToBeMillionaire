using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WhoWantsToBeMillionaire.Properties;

namespace WhoWantsToBeMillionaire
{
    class AchievementShower
    {
        private readonly int _displayTime;
        private readonly int _countFramesMovement;
        private readonly bool[] _isPositionEmpty;
        private readonly Size _sizeBoxAchievement;
        private readonly Queue<Achievement> _queueAchievements;
        private readonly Control _parent;

        public AchievementShower(int widthBoxAchievement, int heightBoxAchievement, int maxCountVisible, int countFramesMovement, int displayTime, Control parent)
        {
            _sizeBoxAchievement = new Size(widthBoxAchievement, heightBoxAchievement);
            _isPositionEmpty = Enumerable.Repeat(true, maxCountVisible).ToArray();
            _queueAchievements = new Queue<Achievement>();
            _countFramesMovement = countFramesMovement;
            _displayTime = displayTime;
            _parent = parent;
        }

        public async void ShowAchievement(Achievement achievement)
        {
            _queueAchievements.Enqueue(achievement);

            var indexEmptyPosition = Array.IndexOf(_isPositionEmpty, true);

            if (indexEmptyPosition > -1)
            {
                _isPositionEmpty[indexEmptyPosition] = false;

                while (_queueAchievements.Count > 0)
                {
                    achievement = _queueAchievements.Dequeue();

                    using (var box = new BoxAchievement(achievement, _sizeBoxAchievement))
                    {
                        box.Location = new Point(-box.Width, indexEmptyPosition * _sizeBoxAchievement.Height);

                        _parent.Controls.Add(box);

                        box.BringToFront();

                        Sound.Play(Resources.Achievement, false);

                        await box.ShowAchievement(_countFramesMovement, _displayTime);

                        _parent.Controls.Remove(box);
                    }
                }

                _isPositionEmpty[indexEmptyPosition] = true;
            }
        }
    }
}