   M S B A M L   `   `   `  ���� K  GBurner_AdvanTech, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null3clr-namespace:Burner_AdvanTechBurner_AdvanTech  !    Burner_AdvanTech.MainWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     0localclr-namespace:Burner_AdvanTech       � Title$#  StringFirmwareUpgradeSoftware��$
��1000��$	��600�� � WindowStartupLocation$ CenterScreen=� � 	Resources  �� MergedDictionaries �� Source��  �� $  Resources\GlassButton.xaml?�%    .���� -    ��Name$ 
tabControl��$��0,0,0,2q� �� SelectedIndex$ 1��.@��� $��Upgrade��.��]� $��0,0,0,1q�.+�� 
�������$	��300��$	��Top=�.+�&   System.Windows.Controls.DataGrid  -     Name$ dataGridFirmwareVersion��$��0��$��0,0,0,0q�   AutoGenerateColumns .  $	��300��$	��top=�	   CanUserAddRows	 .  
   HeadersVisibility$
 Column=�   Columns 0  *System.Windows.Controls.DataGridTextColumn  ,  &System.Windows.Controls.DataGridColumn   Header$ NodeAddress��   Width5  /System.Windows.Controls.DataGridLengthConverter$ * 1  +System.Windows.Controls.DataGridBoundColumn   Binding �� *NodeAddress+4  .System.Windows.Controls.DataGridCheckBoxColumn  $ OnlineStatus��$ *  �� *OnlineStatus+  $ FirmwareVersion��$ *  �� *FirmwareVersion+  $ FirmwareType��$ *  �� *FirmwareType+4  .System.Windows.Controls.DataGridTemplateColumn  $ BurningProgress��$ 2*    CellTemplate �� .R�� .+��  � Minimum$ 0Q� � Maximum$	 100Q� � Value �� *BurningProgress+�� $��center=�$��center=����� *BurningProgress+ =  7System.Windows.Controls.Primitives.DataGridColumnHeader �� 
TargetType �� Property �� Value��%~   '        �� "  .���  � ��
 ���@���  � ��$ SemiBold"���  � ��$ 0,0,1,2q���  � ��
 �   �� 
����  �$��0,3,0,0q�$	��Top=� � ColumnDefinitions �� $��* ��� $��* �.+��� -    ��Name$ StrInfor��$��String Information��$��0��.���� .+��� -    ��Name$ String_Online��$��String_Online��$��0,0,0,5q��� -   $ StringToBooter��$��StringToBooter��$��0,0,0,5q��� -   $ 
StringToAP��$��
StringToAP��$��0,0,0,5q��� -   $ BroadCastBurning��$��BroadCastBurning��$��0,0,0,5q��� -   $ OpenFile��$��OpenFile��$��0,0,0,5q��� -	   $ BpInfor��$��Bp Information��$��1��.���� .+��� -
    ��Name$ StringNumberSelected��$ 0�� �� 
IsEditable .  �� Text$ 	stringall��$��0,0,0,5q��� -    ��Name$ 
BpSelected��$��
BpSelected��$��0,0,0,5q��� -   $ QueryBp_Online��$��QueryBp_Online��$��0,0,0,5q��� -   $ 
BpToBooter��$��
BpToBooter��$��0,0,0,5q��� -   $ BpToAP��$��BpToAP��$��0,0,0,5q��� -    ��Name$ textBoxFilePath��$��0,5,0,5q�
����  �$��24��$	��Top=�]� $	��Top=�.+��� $��Bottom=����� .R��  �� $	��120 ��� $
��Auto ��� $��* ��� $
��Auto ��� $	��120 ��� $
��Auto ��� $	��120 �.@��� .���� -   $��BurnStartTime��$��
StartTime:���� $��1���� $��2��.���� -   $ BurnerFilePath��$��BurnerFilePath���� $��3���� $��4��.���� -   $��BurnStopTime��$��	StopTime:���� $��5���� $��6��.��� -    �Name$ TotalProgressbar��$ 0Q�$ 0Q�$	 100Q�$��90��$��16���� -   $��HinttextBox��$	��Top=� �� AcceptsReturn .   �� TextWrapping$
  Wrap=����� *TextContent+! �� Mode$! OneWay=�.��" � ContextMenu" �� .@�m� -   # m�Name$# 	ClearText��$��Clear��.���� $��Function��.��]� $��	-2,5,2,-5q�.+�� 
�����@�$��0,0,0,2q�$	��Top=�$
��auto�� �� $��* ��� $
��1.5* ��� $��* ��� $
��1.5* �.+��� $��0��$��0,0,5,0q�$��StringController���� -   $ SCNumberselect��$��1���� $��2��$��0,0,5,0q�$��BpController���� -   $ BpNumberselect��$��3��� $	��Top=�
�����@� �� $��* ��� $��* ��� $��* �.+��� $��0��$��Hi/Lo/Ave Value(℃/V)��.���  �� $��* ��� $��* �.+��� $��0��.+��� $��High Vol���� $��Ave Vol���� $��Low Vol���� $��	High Temp���� $��Ave Temp���� $��Low Temp���� $��1��.+��� ���� *highvoltage+�� ���� *
avevoltage+�� ���� *
lowvoltage+�� ���� *
hightemp+�� ���� *	avetemp+�� ���� *	lowtemp+�� $��1��$��Current/Vol Value��.���  �� $��* ��� $��* �.+��� $��0��.+��� $��
String Vol���� $��	DCBUS Vol���� $��String Current���� $��Vol Targert���� $	��KWH���� $��KW���� $��1��.+��� ���� *stringvoltage+�� ���� *dcbusvoltage+�� ���� *stringcurrent+�� ���� *targetvalue+�� ���� *kwh+�� ���� *kw+�� $��2��$��Contactors Status��.���  �� $��* ��� $��* �.+��� $��0��.+��� $��P-ContactorStatus���� $��N-ContactorsStatus���� $��PermissionStatus���� $��1��.+��� ���� *pcontactorsattus+�� ���� *ncontactorstatus+�� ���� *!contactorclosedpermissionstatus+� $	��Top=�.+�  -   $ CellinfodataGridView��$��0��$��0,0,0,0q� .  $	��310��$	��top=�$   
IsReadOnly$ .  	 .  $
 Column=�   $ 
CellNumber��$ *  �� *
CellNumber+  $	 Vol��$ *  �� *Vol+  $
 Temp��$ *  �� *Temp+  $ BalState��$ *  �� *
BalState+ ��%~   '        �� "  .���  � ��
 ���@���  � ��$ SemiBold"���  � ��$ 0,0,1,2q���  � ��
 �   �� $	��320��$	��top=�$��0,2,0,0q�
������� �� $��* ��� $��* �.+��� $��0��$��NormalOperation��.���� .+��� $��Turn on Contactors��$��0,0,0,2q��� $��Turn off Contactors��$��0,0,0,2q��� $��Turn on chargers��$��0,0,0,2q��� $��Send closed Permission��$��0,0,0,2q��� $��Recall closed Permission��$��0,0,0,2q��� $��1��$��BPselfCheck��.���  �� $��* ��� $��* ��� $��* ��� $��* ��� $��* �% � RowDefinitions% �� $��* ��� $��* ��� $��* ��� $��* ��� $��* �.+��� $��0��$��0��   GlassButton#��[  $	��BP1��$��50��$��50��
���� �$��0,0,0,0q��� $��1��$��0��#��[  $	��BP2��$��50��$��50��
���� ��� $��2��$��0��#��[  $	��BP3��$��50��$��50��
���� ��� $��3��$��0��#��[  $	��BP4��$��50��$��50��
���� ��� $��4��$��0��#��[  $	��BP5��$��50��$��50��
���� ��� $��0��$��1��#��[  $	��BP6��$��50��$��50��
���� �$��0,0,0,0q��� $��1��$��1��#��[  $	��BP7��$��50��$��50��
���� ��� $��2��$��1��#��[  $	��BP8��$��50��$��50��
���� ��� $��3��$��1��#��[  $	��BP9��$��50��$��50��
���� ��� $��4��$��1��#��[  $
��BP10��$��50��$��50��
���� ��� $��0��$��2��#��[  $
��BP11��$��50��$��50��
���� �$��0,0,0,0q��� $��1��$��2��#��[  $
��BP12��$��50��$��50��
���� ��� $��2��$��2��#��[  $
��BP13��$��50��$��50��
���� ��� $��3��$��2��#��[  $
��BP14��$��50��$��50��
���� ��� $��4��$��2��#��[  $
��BP15��$��50��$��50��
���� ��� $��0��$��3��#��[  $
��BP16��$��50��$��50��
���� �$��0,0,0,0q��� $��1��$��3��#��[  $
��BP17��$��50��$��50��
���� ��� $��2��$��3��#��[  $
��BP18��$��50��$��50��
���� ��� $��3��$��3��#��[  $
��BP19��$��50��$��50��
���� ��� $��4��$��3��#��[  $
��BP20��$��50��$��50��
���� �]� ��.  
�������$	��Top=�$��0,0,0,0q�$
��auto��& J� RenderTransformOrigin$& 0.5,0.5&�.+��� $��Bottom=�$��25��
����������� .R��  �� $��* ��� $
��auto ��� $��* ��� $
��auto ��� $��* ��� $
��auto ��� $��* �.@��� $��0��.���� -   $ EnableListencheckBox��$��Enable ListenMode���� $��1���� $��2��.���� -   $ EnableBalancingcheckBox��$��Enable Balancing���� $��3���� $��4��.���� -   $ CanPortControlButton��$��Start��$	��133��
��������� $��5���� $��6��.���  �� $��* ��� $��* �.+��� $��	TargetVol��$��0��
�������$��0,0,2,0q��� $��	TargetVol��$��1��
�������$��0,0,2,0q�$��17��$	��Top=�