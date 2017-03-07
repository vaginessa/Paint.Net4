namespace PaintDotNet.Tools.Controls
{
    using PaintDotNet;
    using PaintDotNet.ObjectModel;
    using PaintDotNet.UI.Media;
    using System;
    using System.Windows;

    internal abstract class EditTransformToken : DependencyObject
    {
        private TransformEditingMode editingMode;
        private bool isDeactivated;
        private Transform oldDeltaTransform;
        private PointDouble? oldRotationAnchorOffset;
        private object tag;

        protected EditTransformToken(TransformEditingMode editingMode, Transform oldDeltaTransform, PointDouble? oldRotationAnchorOffset)
        {
            if (editingMode == TransformEditingMode.None)
            {
                throw ExceptionUtil.InvalidEnumArgumentException<TransformEditingMode>(editingMode, "editingMode");
            }
            this.editingMode = editingMode;
            if (oldDeltaTransform == null)
            {
                this.oldDeltaTransform = Transform.Identity;
            }
            else
            {
                this.oldDeltaTransform = oldDeltaTransform.ToFrozen<Transform>();
            }
            this.oldRotationAnchorOffset = oldRotationAnchorOffset;
        }

        public void Cancel()
        {
            this.VerifyIsActive();
            this.OnCancel();
            this.VerifyIsNotActive();
            this.Deactivated();
        }

        public void Commit()
        {
            this.VerifyIsActive();
            this.OnCommit();
            this.VerifyIsNotActive();
            this.Deactivated();
        }

        private void Deactivated()
        {
            base.VerifyAccess();
            if (!this.isDeactivated)
            {
                this.isDeactivated = true;
                this.OnDeactivated();
            }
        }

        protected abstract Transform GetEditTransform();
        protected abstract bool GetIsActive();
        protected abstract PointDouble? GetRotationAnchorOffset();
        public virtual void NotifyDeactivated()
        {
            this.VerifyIsNotActive();
            this.Deactivated();
        }

        protected abstract void OnCancel();
        protected abstract void OnCommit();
        protected abstract void OnDeactivated();
        protected abstract void SetEditTransform(Transform value);
        protected abstract void SetRotationAnchorOffset(PointDouble? value);
        protected virtual void VerifyIsActive()
        {
            base.VerifyAccess();
            if (!this.IsActive)
            {
                ExceptionUtil.ThrowInvalidOperationException();
            }
        }

        protected void VerifyIsNotActive()
        {
            base.VerifyAccess();
            if (this.IsActive)
            {
                ExceptionUtil.ThrowInvalidOperationException();
            }
        }

        public TransformEditingMode EditingMode =>
            this.editingMode;

        public Transform EditTransform
        {
            get => 
                this.GetEditTransform();
            set
            {
                this.VerifyIsActive();
                this.SetEditTransform(value);
                this.VerifyIsActive();
            }
        }

        public bool IsActive
        {
            get
            {
                base.VerifyAccess();
                return this.GetIsActive();
            }
        }

        public Transform OldDeltaTransform =>
            this.oldDeltaTransform;

        public PointDouble? OldRotationAnchorOffset =>
            this.oldRotationAnchorOffset;

        public PointDouble? RotationAnchorOffset
        {
            get => 
                this.GetRotationAnchorOffset();
            set
            {
                this.VerifyIsActive();
                this.SetRotationAnchorOffset(value);
                this.VerifyIsActive();
            }
        }

        public object Tag
        {
            get => 
                this.tag;
            set
            {
                this.tag = value;
            }
        }
    }
}

