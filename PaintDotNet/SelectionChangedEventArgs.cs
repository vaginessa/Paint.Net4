namespace PaintDotNet
{
    public sealed class SelectionChangedEventArgs : PooledEventArgs<SelectionChangedEventArgs, SelectionChangeFlags>
    {
        public static SelectionChangedEventArgs Get(SelectionChangeFlags changeFlags) => 
            PooledEventArgs<SelectionChangedEventArgs, SelectionChangeFlags>.Get(changeFlags);

        public SelectionChangeFlags ChangeFlags =>
            base.Value1;
    }
}

