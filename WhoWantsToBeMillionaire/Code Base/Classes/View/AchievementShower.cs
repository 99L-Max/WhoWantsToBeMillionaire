using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WhoWantsToBeMillionaire
{
    class AchievementShower
    {
        private readonly int _displayTime;
        private readonly int _framesMovementCount;
        private readonly bool[] _isPositionsEmpty;
        private readonly Size _sizeBoxAchievement;
        private readonly Queue<Achievements> _queueAchievements;
        private readonly Control _parent;

        public AchievementShower(float fractionScreenHeight, int widthFraction, int heightFraction, int maxCountVisible, int countFramesMovement, int displayTime, Control parent)
        {
            _sizeBoxAchievement = Resizer.Resize(BasicSizes.Height, (int)(fractionScreenHeight * GameConst.ScreenSize.Height), widthFraction, heightFraction);
            _isPositionsEmpty = Enumerable.Repeat(true, maxCountVisible).ToArray();
            _queueAchievements = new Queue<Achievements>();
            _framesMovementCount = countFramesMovement;
            _displayTime = displayTime;
            _parent = parent;
        }

        public async void ShowAchievement(Achievements achievement)
        {
            _queueAchievements.Enqueue(achievement);

            var indexEmptyPosition = Array.IndexOf(_isPositionsEmpty, true);

            if (indexEmptyPosition > -1)
            {
                _isPositionsEmpty[indexEmptyPosition] = false;

                while (_queueAchievements.Count > 0)
                {
                    achievement = _queueAchievements.Dequeue();

                    using (var box = new BoxAchievement(achievement, _sizeBoxAchievement))
                    {
                        box.Location = new Point(-box.Width, indexEmptyPosition * _sizeBoxAchievement.Height);

                        _parent.Controls.Add(box);

                        box.BringToFront();

                        await box.ShowAchievement(_framesMovementCount, _displayTime);

                        _parent.Controls.Remove(box);
                    }
                }

                _isPositionsEmpty[indexEmptyPosition] = true;
            }
        }
    }
}