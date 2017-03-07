namespace PaintDotNet.Tools
{
    using PaintDotNet;
    using PaintDotNet.Diagnostics;
    using System;

    internal abstract class TransactedToolAgent<TTransactionToken> where TTransactionToken: TransactedToolToken
    {
        private string name;
        private TTransactionToken transactionToken;

        protected TransactedToolAgent(string name)
        {
            Validate.IsNotNullOrWhiteSpace(name, "name");
            this.name = name;
        }

        public void VerifyIsActive()
        {
            if (!this.IsActive)
            {
                ExceptionUtil.ThrowInvalidOperationException("This operation may only be performed when IsActive is true");
            }
        }

        public void VerifyIsNotActive()
        {
            if (this.IsActive)
            {
                ExceptionUtil.ThrowInvalidOperationException("This operation may only be performed when IsActive is false");
            }
        }

        public bool IsActive =>
            this.TransactionToken.IsActive();

        public string Name =>
            this.name;

        public TTransactionToken TransactionToken
        {
            get => 
                this.transactionToken;
            set
            {
                if ((value != null) && this.transactionToken.IsActive())
                {
                    ExceptionUtil.ThrowInvalidOperationException("Cannot replace a token that is still active");
                }
                this.transactionToken = value;
            }
        }
    }
}

