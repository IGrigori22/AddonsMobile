using Microsoft.Xna.Framework;

namespace AddonsMobile.UI.Animation
{
    /// <summary>
    /// Mengontrol semua animasi untuk FAB dan Menu Bar
    /// </summary>
    public class AnimationController
    {
        // Gear Rotation
        private float _gearRotation = 0f;
        private float _targetRotation = 0f;
        private bool _isRotating = false;

        // Expand Animation
        private float _expandProgress = 0f;
        private bool _isAnimating = false;
        private bool _isExpanded = false;

        // Menu Bar Animation
        private float _menuBarWidth = 0f;
        private float _targetMenuBarWidth = 0f;
        private float _menuBarOpacity = 0f;

        // Constants
        private const float ROTATION_SPEED = 8f;
        private const float ROTATION_AMOUNT = MathHelper.TwoPi;
        private const float MENU_ANIMATION_SPEED = 6f;

        // Public Properties
        public float GearRotation => _gearRotation;
        public float ExpandProgress => _expandProgress;
        public float MenuBarWidth => _menuBarWidth;
        public float MenuBarOpacity => _menuBarOpacity;
        public bool IsExpanded => _isExpanded;
        public bool IsAnimating => _isAnimating;
        public float TargetMenuBarWidth => _targetMenuBarWidth;

        public void StartGearRotation()
        {
            _isRotating = true;
            _targetRotation = _gearRotation + ROTATION_AMOUNT;
        }

        public void SetExpanded(bool expanded)
        {
            _isExpanded = expanded;
            _isAnimating = true;

            if (!expanded)
            {
                _targetMenuBarWidth = 0f;
            }
        }

        public void SetTargetMenuBarWidth(float width)
        {
            _targetMenuBarWidth = width;
        }

        public void Reset()
        {
            _isExpanded = false;
            _expandProgress = 0f;
            _menuBarOpacity = 0f;
            _menuBarWidth = 0f;
            _gearRotation = 0f;
            _isRotating = false;
            _isAnimating = false;
        }

        public void Update(float deltaTime, bool isDragging)
        {
            UpdateGearRotation(deltaTime, isDragging);
            UpdateExpandAnimation(deltaTime);
            UpdateMenuBarAnimation(deltaTime);
        }

        private void UpdateGearRotation(float deltaTime, bool isDragging)
        {
            if (_isRotating)
            {
                float rotationDelta = ROTATION_SPEED * deltaTime;

                if (_gearRotation < _targetRotation)
                {
                    _gearRotation += rotationDelta;

                    if (_gearRotation >= _targetRotation)
                    {
                        _gearRotation = _targetRotation;
                        _isRotating = false;

                        while (_gearRotation >= MathHelper.TwoPi)
                        {
                            _gearRotation -= MathHelper.TwoPi;
                            _targetRotation -= MathHelper.TwoPi;
                        }
                    }
                }
            }

            // Continuous slow rotation when expanded
            if (_isExpanded && !_isRotating)
            {
                _gearRotation += deltaTime * 0.5f;
                if (_gearRotation >= MathHelper.TwoPi)
                {
                    _gearRotation -= MathHelper.TwoPi;
                }
            }

            // Faster rotation when dragging
            if (isDragging)
            {
                _gearRotation += deltaTime * 3f;
                if (_gearRotation >= MathHelper.TwoPi)
                {
                    _gearRotation -= MathHelper.TwoPi;
                }
            }
        }

        private void UpdateExpandAnimation(float deltaTime)
        {
            if (!_isAnimating) return;

            float animationSpeed = MENU_ANIMATION_SPEED * deltaTime;

            if (_isExpanded)
            {
                _expandProgress = Math.Min(1f, _expandProgress + animationSpeed);
                if (_expandProgress >= 1f) _isAnimating = false;
            }
            else
            {
                _expandProgress = Math.Max(0f, _expandProgress - animationSpeed);
                if (_expandProgress <= 0f) _isAnimating = false;
            }
        }

        private void UpdateMenuBarAnimation(float deltaTime)
        {
            float animationSpeed = MENU_ANIMATION_SPEED * deltaTime;

            if (_isExpanded)
            {
                _menuBarWidth = MathHelper.Lerp(_menuBarWidth, _targetMenuBarWidth, animationSpeed * 2);
                _menuBarOpacity = MathHelper.Lerp(_menuBarOpacity, 1f, animationSpeed * 2);
            }
            else
            {
                _menuBarWidth = MathHelper.Lerp(_menuBarWidth, 0f, animationSpeed * 2);
                _menuBarOpacity = MathHelper.Lerp(_menuBarOpacity, 0f, animationSpeed * 2);
            }
        }
    }
}