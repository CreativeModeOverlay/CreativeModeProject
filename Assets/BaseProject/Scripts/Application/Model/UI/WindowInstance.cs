namespace CreativeMode
{
    public struct WindowInstance<C, W> 
        where C : IWindowUIContainer 
        where W : IWindowUI
    {
        public C Container;
        public W Window;
    }
}