using Microsoft.Win32;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace NZTS_App.Views
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool booleanValue)
            {
                return booleanValue ? Brushes.Green : Brushes.Red; // Adjust colors as needed
            }
            return Brushes.Transparent; // Default color
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class MemoryMan : UserControl
    {
        private const string MemoryKeyPath = @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management";
        private MainWindow mainWindow;

        public MemoryMan(MainWindow window)
        {
            InitializeComponent();
            mainWindow = window;
            LoadCurrentSettings();
            mainWindow.TitleTextBlock.Content = "Memory";

            DisablePagingExecutiveToggle.Click += DisablePagingExecutiveToggle_Click;
            ContextSwitchDeadbandToggle.Click += ContextSwitchDeadbandToggle_Click;
            LatencySensitivityHintToggle.Click += LatencySensitivityHintToggle_Click;
            DisableHeapCoalesceOnFreeToggle.Click += DisableHeapCoalesceOnFreeToggle_Click;
            LargePageMinimumToggle.Click += LargePageMinimumToggle_Click;
            SecondLevelDataCacheToggle.Click += SecondLevelDataCacheToggle_Click;
            ThirdLevelDataCacheToggle.Click += ThirdLevelDataCacheToggle_Click;
            DisableOSMitigationsToggle.Click += DisableOSMitigationsToggle_Click;
            SystemCacheDirtyPageThresholdToggle.Click += SystemCacheDirtyPageThresholdToggle_Click;
            LargePageSizeInBytesToggle.Click += LargePageSizeInBytesToggle_Click;
            LockPagesInMemoryToggle.Click += LockPagesInMemoryToggle_Click;
            LargePageHeapSizeThresholdToggle.Click += LargePageHeapSizeThresholdToggle_Click;
            UseBiasedLockingToggle.Click += UseBiasedLockingToggle_Click;
            TieredCompilationToggle.Click += TieredCompilationToggle_Click;
            TieredStopAtLevelToggle.Click += TieredStopAtLevelToggle_Click;
            ThreadStackSizeToggle.Click += ThreadStackSizeToggle_Click;
            StrictFileSharingToggle.Click += StrictFileSharingToggle_Click;
            AllowPagedPoolGrowToggle.Click += AllowPagedPoolGrowToggle_Click;
            DataClusterSizeToggle.Click += DataClusterSizeToggle_Click;
            MapIoSpaceToggle.Click += MapIoSpaceToggle_Click;
            SystemVaStartToggle.Click += SystemVaStartToggle_Click;
            SystemVaEndToggle.Click += SystemVaEndToggle_Click;
            TrimThresholdOnMemoryPressureToggle.Click += TrimThresholdOnMemoryPressureToggle_Click;
            LargeSystemCacheToggle.Click += LargeSystemCacheToggle_Click;
            ForceSectionCreationToggle.Click += ForceSectionCreationToggle_Click;
            EnableKernelPageCompressionToggle.Click += EnableKernelPageCompressionToggle_Click;
            CacheClusterSizeToggle.Click += CacheClusterSizeToggle_Click;
            PageFaultCoalescingToggle.Click += PageFaultCoalescingToggle_Click;
            HardFaultThreadPriorityToggle.Click += HardFaultThreadPriorityToggle_Click;
            TlbFlushThresholdToggle.Click += TlbFlushThresholdToggle_Click;
            EnforceCachePartitioningToggle.Click += EnforceCachePartitioningToggle_Click;
            InvalidateTlbOnForkToggle.Click += InvalidateTlbOnForkToggle_Click;
            HotPageThresholdToggle.Click += HotPageThresholdToggle_Click;
            DirectCacheFlushToggle.Click += DirectCacheFlushToggle_Click;
            InterruptDrivenPagingToggle.Click += InterruptDrivenPagingToggle_Click;
            CompressedStoreThresholdToggle.Click += CompressedStoreThresholdToggle_Click;
            CacheLinePrefetchToggle.Click += CacheLinePrefetchToggle_Click;
            PageColorPolicyToggle.Click += PageColorPolicyToggle_Click;
            IoMmuBypassToggle.Click += IoMmuBypassToggle_Click;
            TlbShootdownCoalesceToggle.Click += TlbShootdownCoalesceToggle_Click;
            DynamicPtePromotionToggle.Click += DynamicPtePromotionToggle_Click;
            HardFaultBurstLimitToggle.Click += HardFaultBurstLimitToggle_Click;
            ResidentAvailableThresholdToggle.Click += ResidentAvailableThresholdToggle_Click;
            CacheAllocAlignmentToggle.Click += CacheAllocAlignmentToggle_Click;
            PrefetchDistanceToggle.Click += PrefetchDistanceToggle_Click;
            StaleTlbThresholdToggle.Click += StaleTlbThresholdToggle_Click;
            GpuApertureSizeToggle.Click += GpuApertureSizeToggle_Click;
            WriteCombineGranularityToggle.Click += WriteCombineGranularityToggle_Click;
            DisableSmapToggle.Click += DisableSmapToggle_Click;
            KernelExecutePoolToggle.Click += KernelExecutePoolToggle_Click;
            NullDereferenceProtectionToggle.Click += NullDereferenceProtectionToggle_Click;


        }



        private void LoadCurrentSettings()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(MemoryKeyPath))
                {
                    if (key != null)
                    {
                        // DisablePagingExecutive
                        var disablePagingValue = key.GetValue("DisablePagingExecutive");
                        DisablePagingExecutiveToggle.IsChecked = (disablePagingValue is int disablePagingInt && disablePagingInt == 1);

                        // ContextSwitchDeadband
                        var contextSwitchValue = key.GetValue("ContextSwitchDeadband");
                        ContextSwitchDeadbandToggle.IsChecked = (contextSwitchValue is int && (int)contextSwitchValue == 1);

                        // LatencySensitivityHint
                        var latencyHintValue = key.GetValue("LatencySensitivityHint");
                        LatencySensitivityHintToggle.IsChecked = (latencyHintValue is int && (int)latencyHintValue == 1);

                        // DisableHeapCoalesceOnFree
                        var heapCoalesceValue = key.GetValue("DisableHeapCoalesceOnFree");
                        DisableHeapCoalesceOnFreeToggle.IsChecked = (heapCoalesceValue is int disableHeapInt && disableHeapInt == 1);

                        var systemCacheDirtyPageThresholdValue = key.GetValue("SystemCacheDirtyPageThreshold");
                        SystemCacheDirtyPageThresholdToggle.IsChecked = systemCacheDirtyPageThresholdValue != null && (int)systemCacheDirtyPageThresholdValue == 3;

                        // StrictFileSharing
                        var strictFileSharingValue = key.GetValue("StrictFileSharing");
                        StrictFileSharingToggle.IsChecked = (strictFileSharingValue is int && (int)strictFileSharingValue == 1);

                        // AllowPagedPoolGrow
                        var allowPagedPoolGrowValue = key.GetValue("AllowPagedPoolGrow");
                        AllowPagedPoolGrowToggle.IsChecked = (allowPagedPoolGrowValue is int && (int)allowPagedPoolGrowValue == 1);

                        // DataClusterSize
                        var dataClusterSizeValue = key.GetValue("DataClusterSize");
                        DataClusterSizeToggle.IsChecked = (dataClusterSizeValue is int && (int)dataClusterSizeValue == 0x128); // 0x128 = 296

                        // MapIoSpace
                        var mapIoSpaceValue = key.GetValue("MapIoSpace");
                        MapIoSpaceToggle.IsChecked = (mapIoSpaceValue is int && (int)mapIoSpaceValue == 2);

                        // SystemVaStart
                        var systemVaStartValue = key.GetValue("SystemVaStart");
                        SystemVaStartToggle.IsChecked = (systemVaStartValue is int && (int)systemVaStartValue == 0x989680); // 0x989680 = 10000000

                        // SystemVaEnd
                        var systemVaEndValue = key.GetValue("SystemVaEnd");
                        SystemVaEndToggle.IsChecked = (systemVaEndValue is int && (int)systemVaEndValue == 0x4C4B400); // 0x4C4B400 = 80000000

                        // TrimThresholdOnMemoryPressure
                        var trimThresholdOnMemoryPressureValue = key.GetValue("TrimThresholdOnMemoryPressure");
                        TrimThresholdOnMemoryPressureToggle.IsChecked = (trimThresholdOnMemoryPressureValue is int && (int)trimThresholdOnMemoryPressureValue == 0x10); // 0x10 = 16



                        // MmLargeSystemCache
                        var mmLargeSystemCacheValue = key.GetValue("MmLargeSystemCache");
                        LargeSystemCacheToggle.IsChecked = (mmLargeSystemCacheValue is int && (int)mmLargeSystemCacheValue == 0);

                        // MmForceSectionCreation
                        var mmForceSectionCreationValue = key.GetValue("MmForceSectionCreation");
                        ForceSectionCreationToggle.IsChecked = (mmForceSectionCreationValue is int && (int)mmForceSectionCreationValue == 1);

                        // MmEnableKernelPageCompression
                        var mmEnableKernelPageCompressionValue = key.GetValue("MmEnableKernelPageCompression");
                        EnableKernelPageCompressionToggle.IsChecked = (mmEnableKernelPageCompressionValue is int && (int)mmEnableKernelPageCompressionValue == 0);

                        // MmCacheClusterSize
                        var mmCacheClusterSizeValue = key.GetValue("MmCacheClusterSize");
                        CacheClusterSizeToggle.IsChecked = (mmCacheClusterSizeValue is int && (int)mmCacheClusterSizeValue == 0x40);

                        // MmPageFaultCoalescing
                        var mmPageFaultCoalescingValue = key.GetValue("MmPageFaultCoalescing");
                        PageFaultCoalescingToggle.IsChecked = (mmPageFaultCoalescingValue is int && (int)mmPageFaultCoalescingValue == 1);

                        // MmHardFaultThreadPriority
                        var mmHardFaultThreadPriorityValue = key.GetValue("MmHardFaultThreadPriority");
                        HardFaultThreadPriorityToggle.IsChecked = (mmHardFaultThreadPriorityValue is int && (int)mmHardFaultThreadPriorityValue == 0x27);

                        // MmTlbFlushThreshold
                        var mmTlbFlushThresholdValue = key.GetValue("MmTlbFlushThreshold");
                        TlbFlushThresholdToggle.IsChecked = (mmTlbFlushThresholdValue is int && (int)mmTlbFlushThresholdValue == 0x400);

                        // MmEnforceCachePartitioning
                        var mmEnforceCachePartitioningValue = key.GetValue("MmEnforceCachePartitioning");
                        EnforceCachePartitioningToggle.IsChecked = (mmEnforceCachePartitioningValue is int && (int)mmEnforceCachePartitioningValue == 1);

                        // MmInvalidateTlbOnFork
                        var mmInvalidateTlbOnForkValue = key.GetValue("MmInvalidateTlbOnFork");
                        InvalidateTlbOnForkToggle.IsChecked = (mmInvalidateTlbOnForkValue is int && (int)mmInvalidateTlbOnForkValue == 1);

                        // MmHotPageThreshold
                        var mmHotPageThresholdValue = key.GetValue("MmHotPageThreshold");
                        HotPageThresholdToggle.IsChecked = (mmHotPageThresholdValue is int && (int)mmHotPageThresholdValue == 0x1000); // 0x1000 = dword:00001000

                        // MmDirectCacheFlush
                        var mmDirectCacheFlushValue = key.GetValue("MmDirectCacheFlush");
                        DirectCacheFlushToggle.IsChecked = (mmDirectCacheFlushValue is int && (int)mmDirectCacheFlushValue == 0);

                        // MmInterruptDrivenPaging
                        var mmInterruptDrivenPagingValue = key.GetValue("MmInterruptDrivenPaging");
                        InterruptDrivenPagingToggle.IsChecked = (mmInterruptDrivenPagingValue is int && (int)mmInterruptDrivenPagingValue == 1);

                        // MmCompressedStoreThreshold
                        var mmCompressedStoreThresholdValue = key.GetValue("MmCompressedStoreThreshold");
                        CompressedStoreThresholdToggle.IsChecked = (mmCompressedStoreThresholdValue is int && (int)mmCompressedStoreThresholdValue == 1);

                        // MmCacheLinePrefetch
                        var mmCacheLinePrefetchValue = key.GetValue("MmCacheLinePrefetch");
                        CacheLinePrefetchToggle.IsChecked = (mmCacheLinePrefetchValue is int && (int)mmCacheLinePrefetchValue == 0x03);

                        // MmPageColorPolicy
                        var mmPageColorPolicyValue = key.GetValue("MmPageColorPolicy");
                        PageColorPolicyToggle.IsChecked = (mmPageColorPolicyValue is int && (int)mmPageColorPolicyValue == 0x02);

                        // MmIoMmuBypass
                        var mmIoMmuBypassValue = key.GetValue("MmIoMmuBypass");
                        IoMmuBypassToggle.IsChecked = (mmIoMmuBypassValue is int && (int)mmIoMmuBypassValue == 1);

                        // MmTlbShootdownCoalesce
                        var mmTlbShootdownCoalesceValue = key.GetValue("MmTlbShootdownCoalesce");
                        TlbShootdownCoalesceToggle.IsChecked = (mmTlbShootdownCoalesceValue is int && (int)mmTlbShootdownCoalesceValue == 0x50);

                        // MmDynamicPtePromotion
                        var mmDynamicPtePromotionValue = key.GetValue("MmDynamicPtePromotion");
                        DynamicPtePromotionToggle.IsChecked = (mmDynamicPtePromotionValue is int && (int)mmDynamicPtePromotionValue == 1);

                        // MmHardFaultBurstLimit
                        var mmHardFaultBurstLimitValue = key.GetValue("MmHardFaultBurstLimit");
                        HardFaultBurstLimitToggle.IsChecked = (mmHardFaultBurstLimitValue is int && (int)mmHardFaultBurstLimitValue == 0);

                        // MmResidentAvailableThreshold
                        var mmResidentAvailableThresholdValue = key.GetValue("MmResidentAvailableThreshold");
                        ResidentAvailableThresholdToggle.IsChecked = (mmResidentAvailableThresholdValue is int && (int)mmResidentAvailableThresholdValue == 0x90);

                        // MmCacheAllocAlignment
                        var mmCacheAllocAlignmentValue = key.GetValue("MmCacheAllocAlignment");
                        CacheAllocAlignmentToggle.IsChecked = (mmCacheAllocAlignmentValue is int && (int)mmCacheAllocAlignmentValue == 0x40);

                        // MmPrefetchDistance
                        var mmPrefetchDistanceValue = key.GetValue("MmPrefetchDistance");
                        PrefetchDistanceToggle.IsChecked = (mmPrefetchDistanceValue is int && (int)mmPrefetchDistanceValue == 0x04);

                        // MmStaleTlbThreshold
                        var mmStaleTlbThresholdValue = key.GetValue("MmStaleTlbThreshold");
                        StaleTlbThresholdToggle.IsChecked = (mmStaleTlbThresholdValue is int && (int)mmStaleTlbThresholdValue == 0x400);

                        // MmGpuApertureSize
                        var mmGpuApertureSizeValue = key.GetValue("MmGpuApertureSize");
                        GpuApertureSizeToggle.IsChecked = (mmGpuApertureSizeValue is int && (int)mmGpuApertureSizeValue == 0x8000);

                        // MmWriteCombineGranularity
                        var mmWriteCombineGranularityValue = key.GetValue("MmWriteCombineGranularity");
                        WriteCombineGranularityToggle.IsChecked = (mmWriteCombineGranularityValue is int && (int)mmWriteCombineGranularityValue == 0x200);

                        // MmDisableSmap
                        var mmDisableSmapValue = key.GetValue("MmDisableSmap");
                        DisableSmapToggle.IsChecked = (mmDisableSmapValue is int && (int)mmDisableSmapValue == 1);

                        // MmKernelExecutePool
                        var mmKernelExecutePoolValue = key.GetValue("MmKernelExecutePool");
                        KernelExecutePoolToggle.IsChecked = (mmKernelExecutePoolValue is int && (int)mmKernelExecutePoolValue == 0);

                        // MmNullDereferenceProtection
                        var mmNullDereferenceProtectionValue = key.GetValue("MmNullDereferenceProtection");
                        NullDereferenceProtectionToggle.IsChecked = (mmNullDereferenceProtectionValue is int && (int)mmNullDereferenceProtectionValue == 0);

                        // LargePageMinimum
                        var largePageMinimumValue = key.GetValue("LargePageMinimum");
                        LargePageMinimumToggle.IsChecked = (largePageMinimumValue is int largePageMinInt && largePageMinInt == unchecked((int)0xFFFFFFFF));

                        var secondLevelCacheValue = key.GetValue("SecondLevelDataCache");
                        SecondLevelDataCacheToggle.IsChecked = secondLevelCacheValue != null;

                        // ThirdLevelDataCache
                        var thirdLevelCacheValue = key.GetValue("ThirdLevelDataCache");
                        ThirdLevelDataCacheToggle.IsChecked = thirdLevelCacheValue != null;

                        // LargePageSizeInBytes
                        var LargePageSizeInByteseValue = key.GetValue("LargePageSizeInBytes");
                        LargePageSizeInBytesToggle.IsChecked = LargePageSizeInByteseValue != null;

                        // LockPagesInMemoryValue
                        var LockPagesInMemoryValue = key.GetValue("LockPagesInMemory");
                        LockPagesInMemoryToggle.IsChecked = LockPagesInMemoryValue != null;

                        // LargePageHeapSizeThreshold
                        var LargePageHeapSizeThreshold = key.GetValue("LargePageHeapSizeThreshold");
                        LargePageHeapSizeThresholdToggle.IsChecked = LargePageHeapSizeThreshold != null;

                        // UseBiasedLocking
                        var UseBiasedLockingValue = key.GetValue("UseBiasedLocking");
                        UseBiasedLockingToggle.IsChecked = UseBiasedLockingValue != null;

                        // TieredCompilation
                        var TieredCompilationValue = key.GetValue("TieredCompilation");
                        TieredCompilationToggle.IsChecked = TieredCompilationValue != null;

                        // TieredStopAtLevel
                        var TieredStopAtLevelValue = key.GetValue("TieredStopAtLevel");
                        TieredStopAtLevelToggle.IsChecked = TieredStopAtLevelValue != null;

                        // ThreadStackSize
                        var ThreadStackSizeValue = key.GetValue("ThreadStackSize");
                        ThreadStackSizeToggle.IsChecked = ThreadStackSizeValue != null;

                        // Mitigations
                        var DisableOSMitigationsValue = key.GetValue("FeatureSettingsOverride");
                        var DisableOSMitigationsMask = key.GetValue("FeatureSettingsOverrideMask");

                        // Check if FeatureSettingsOverride is 0x00000048 (default value)
                        if (DisableOSMitigationsValue is int disableOSMitigations && disableOSMitigations == 0x00000048)
                        {
                            // Set the toggle off if the value is 0x00000048
                            DisableOSMitigationsToggle.IsChecked = false;
                        }
                        else
                        {
                            // Use existing logic if FeatureSettingsOverride is not the default value
                            DisableOSMitigationsToggle.IsChecked = DisableOSMitigationsValue != null && DisableOSMitigationsMask != null;
                        }

                    }
                    else
                    {
                        ShowError("Failed to access Memory Management registry key.");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("You do not have permission to access the registry key. Please run the application as an administrator.");
            }
            catch (Exception ex)
            {
                ShowError($"Error loading current settings: {ex.Message}");
            }
        }

        private void SwitchToVerifiedTab(object sender, RoutedEventArgs e)
        {
            VerifiedContent.Visibility = Visibility.Visible;
            ExperimentalContent.Visibility = Visibility.Collapsed;

            // Update the active tag
            VerifiedButton.Tag = "Active";
            ExperimentalButton.Tag = "Inactive";
        }

        private void SwitchToExperimentalTab(object sender, RoutedEventArgs e)
        {
            VerifiedContent.Visibility = Visibility.Collapsed;
            ExperimentalContent.Visibility = Visibility.Visible;

            // Update the active tag
            ExperimentalButton.Tag = "Active";
            VerifiedButton.Tag = "Inactive";
        }

        private void SystemCacheDirtyPageThresholdToggle_Click(object sender, RoutedEventArgs e)
        {
            if (SystemCacheDirtyPageThresholdToggle.IsChecked == true)
            {
                UpdateRegistryValue("SystemCacheDirtyPageThreshold", 3); // Set to tweaked value
            }
            else
            {
                DeleteRegistryValue("SystemCacheDirtyPageThreshold"); // Delete if toggled off (reset to original)
            }
        }


        private void SecondLevelDataCacheToggle_Click(object sender, RoutedEventArgs e)
        {
            if (SecondLevelDataCacheToggle.IsChecked == true)
            {
                UpdateRegistryValue("SecondLevelDataCache", 0x00FA332A); // Set to default value
            }
            else
            {
                DeleteRegistryValue("SecondLevelDataCache");
            }
        }

        private void ThirdLevelDataCacheToggle_Click(object sender, RoutedEventArgs e)
        {
            if (ThirdLevelDataCacheToggle.IsChecked == true)
            {
                UpdateRegistryValue("ThirdLevelDataCache", 0x00FA332A); // Set to default value
            }
            else
            {
                DeleteRegistryValue("ThirdLevelDataCache");
            }
        }

        private void DisablePagingExecutiveToggle_Click(object sender, RoutedEventArgs e)
        {
            UpdateRegistryValue("DisablePagingExecutive", DisablePagingExecutiveToggle.IsChecked == true ? 1 : 0);
        }

        private void ContextSwitchDeadbandToggle_Click(object sender, RoutedEventArgs e)
        {
            if (ContextSwitchDeadbandToggle.IsChecked == true)
            {
                UpdateRegistryValue("ContextSwitchDeadband", 1);
            }
            else
            {
                DeleteRegistryValue("ContextSwitchDeadband");
            }
        }

        private void LatencySensitivityHintToggle_Click(object sender, RoutedEventArgs e)
        {
            if (LatencySensitivityHintToggle.IsChecked == true)
            {
                UpdateRegistryValue("LatencySensitivityHint", 1);
            }
            else
            {
                DeleteRegistryValue("LatencySensitivityHint");
            }
        }



        private void DisableHeapCoalesceOnFreeToggle_Click(object sender, RoutedEventArgs e)
        {
            UpdateRegistryValue("DisableHeapCoalesceOnFree", DisableHeapCoalesceOnFreeToggle.IsChecked == true ? 1 : 0);
        }

        private void LargePageMinimumToggle_Click(object sender, RoutedEventArgs e)
        {
            if (LargePageMinimumToggle.IsChecked == true)
            {
                UpdateRegistryValue("LargePageMinimum", unchecked((int)0xFFFFFFFF)); // Set to 0xFFFFFFFF
            }
            else
            {
                DeleteRegistryValue("LargePageMinimum");
            }
        }

        private void DisableOSMitigationsToggle_Click(object sender, RoutedEventArgs e)
        {
            if (DisableOSMitigationsToggle.IsChecked == true)
            {
                // Set to tweaked values
                UpdateRegistryValue("FeatureSettingsOverride", 0x00000003);  // Tweaked value for FeatureSettingsOverride
                UpdateRegistryValue("FeatureSettingsOverrideMask", 0x00000003);  // Tweaked value for FeatureSettingsOverrideMask
            }
            else
            {
                // Set to default values (default value for FeatureSettingsOverride is 0x00000048, FeatureSettingsOverrideMask is 0x00000003)
                UpdateRegistryValue("FeatureSettingsOverride", 0x00000048);  // Default value for FeatureSettingsOverride
                UpdateRegistryValue("FeatureSettingsOverrideMask", 0x00000003);  // Default value for FeatureSettingsOverrideMask
            }
        }

        private void LargePageSizeInBytesToggle_Click(object sender, RoutedEventArgs e)
        {
            if (LargePageSizeInBytesToggle.IsChecked == true)
            {
                UpdateRegistryValue("LargePageSizeInBytes", 3);
            }
            else
            {
                DeleteRegistryValue("LargePageSizeInBytes");
            }
        }

        private void StrictFileSharingToggle_Click(object sender, RoutedEventArgs e)
        {
            if (StrictFileSharingToggle.IsChecked == true)
            {
                // Update both Mm and non-Mm registry values
                UpdateRegistryValue("StrictFileSharing", 1);
                UpdateRegistryValue("MmStrictFileSharing", 1);
            }
            else
            {
                // Delete both Mm and non-Mm registry values
                DeleteRegistryValue("StrictFileSharing");
                DeleteRegistryValue("MmStrictFileSharing");
            }
        }


        private void AllowPagedPoolGrowToggle_Click(object sender, RoutedEventArgs e)
        {
            if (AllowPagedPoolGrowToggle.IsChecked == true)
            {
                // Update both Mm and non-Mm registry values
                UpdateRegistryValue("AllowPagedPoolGrow", 1);
                UpdateRegistryValue("MmAllowPagedPoolGrow", 1);
            }
            else
            {
                // Delete both Mm and non-Mm registry values
                DeleteRegistryValue("AllowPagedPoolGrow");
                DeleteRegistryValue("MmAllowPagedPoolGrow");
            }
        }


        private void DataClusterSizeToggle_Click(object sender, RoutedEventArgs e)
        {
            if (DataClusterSizeToggle.IsChecked == true)
            {
                // Update both Mm and non-Mm registry values
                UpdateRegistryValue("DataClusterSize", 0x128);  // 0x128 = 296
                UpdateRegistryValue("MmDataClusterSize", 0x128);
            }
            else
            {
                // Delete both Mm and non-Mm registry values
                DeleteRegistryValue("DataClusterSize");
                DeleteRegistryValue("MmDataClusterSize");
            }
        }


        private void MapIoSpaceToggle_Click(object sender, RoutedEventArgs e)
        {
            if (MapIoSpaceToggle.IsChecked == true)
            {
                // Update both Mm and non-Mm registry values
                UpdateRegistryValue("MapIoSpace", 0x2);  // 0x2 corresponds to dword:00000002
                UpdateRegistryValue("MmMapIoSpace", 0x2);
            }
            else
            {
                // Delete both Mm and non-Mm registry values
                DeleteRegistryValue("MapIoSpace");
                DeleteRegistryValue("MmMapIoSpace");
            }
        }


        private void SystemVaStartToggle_Click(object sender, RoutedEventArgs e)
        {
            if (SystemVaStartToggle.IsChecked == true)
            {
                // Update both Mm and non-Mm registry values
                UpdateRegistryValue("SystemVaStart", 0x989680);  // 0x989680 = dword:00989680
                UpdateRegistryValue("MmSystemVaStart", 0x989680);
            }
            else
            {
                // Delete both Mm and non-Mm registry values
                DeleteRegistryValue("SystemVaStart");
                DeleteRegistryValue("MmSystemVaStart");
            }
        }


        private void SystemVaEndToggle_Click(object sender, RoutedEventArgs e)
        {
            if (SystemVaEndToggle.IsChecked == true)
            {
                // Update both Mm and non-Mm registry values
                UpdateRegistryValue("SystemVaEnd", 0x4c4b400);  // 0x4c4b400 = dword:04c4b400
                UpdateRegistryValue("MmSystemVaEnd", 0x4c4b400);
            }
            else
            {
                // Delete both Mm and non-Mm registry values
                DeleteRegistryValue("SystemVaEnd");
                DeleteRegistryValue("MmSystemVaEnd");
            }
        }


        private void TrimThresholdOnMemoryPressureToggle_Click(object sender, RoutedEventArgs e)
        {
            if (TrimThresholdOnMemoryPressureToggle.IsChecked == true)
            {
                // Update both Mm and non-Mm registry values
                UpdateRegistryValue("TrimThresholdOnMemoryPressure", 0x10);  // 0x10 = dword:00000010
                UpdateRegistryValue("MmTrimThresholdOnMemoryPressure", 0x10);
            }
            else
            {
                // Delete both Mm and non-Mm registry values
                DeleteRegistryValue("TrimThresholdOnMemoryPressure");
                DeleteRegistryValue("MmTrimThresholdOnMemoryPressure");
            }
        }

        private void ForceSectionCreationToggle_Click(object sender, RoutedEventArgs e)
        {
            if (ForceSectionCreationToggle.IsChecked == true)
            {
                // Update both Mm and non-Mm registry values
                UpdateRegistryValue("ForceSectionCreation", 1);
                UpdateRegistryValue("MmForceSectionCreation", 1);
            }
            else
            {
                // Delete both Mm and non-Mm registry values
                DeleteRegistryValue("ForceSectionCreation");
                DeleteRegistryValue("MmForceSectionCreation");
            }
        }

        private void EnableKernelPageCompressionToggle_Click(object sender, RoutedEventArgs e)
        {
            if (EnableKernelPageCompressionToggle.IsChecked == true)
            {
                // Update both Mm and non-Mm registry values
                UpdateRegistryValue("EnableKernelPageCompression", 0);
                UpdateRegistryValue("MmEnableKernelPageCompression", 0);
            }
            else
            {
                // Delete both Mm and non-Mm registry values
                DeleteRegistryValue("EnableKernelPageCompression");
                DeleteRegistryValue("MmEnableKernelPageCompression");
            }
        }

        private void CacheClusterSizeToggle_Click(object sender, RoutedEventArgs e)
        {
            if (CacheClusterSizeToggle.IsChecked == true)
            {
                // Update both Mm and non-Mm registry values
                UpdateRegistryValue("CacheClusterSize", 0x40);  // 0x40 = dword:00000040
                UpdateRegistryValue("MmCacheClusterSize", 0x40);
            }
            else
            {
                // Delete both Mm and non-Mm registry values
                DeleteRegistryValue("CacheClusterSize");
                DeleteRegistryValue("MmCacheClusterSize");
            }
        }

        private void PageFaultCoalescingToggle_Click(object sender, RoutedEventArgs e)
        {
            if (PageFaultCoalescingToggle.IsChecked == true)
            {
                // Update both Mm and non-Mm registry values
                UpdateRegistryValue("PageFaultCoalescing", 1);
                UpdateRegistryValue("MmPageFaultCoalescing", 1);
            }
            else
            {
                // Delete both Mm and non-Mm registry values
                DeleteRegistryValue("PageFaultCoalescing");
                DeleteRegistryValue("MmPageFaultCoalescing");
            }
        }

        private void HardFaultThreadPriorityToggle_Click(object sender, RoutedEventArgs e)
        {
            if (HardFaultThreadPriorityToggle.IsChecked == true)
            {
                // Update both Mm and non-Mm registry values
                UpdateRegistryValue("HardFaultThreadPriority", 0x27);  // 0x27 = dword:00000027
                UpdateRegistryValue("MmHardFaultThreadPriority", 0x27);
            }
            else
            {
                // Delete both Mm and non-Mm registry values
                DeleteRegistryValue("HardFaultThreadPriority");
                DeleteRegistryValue("MmHardFaultThreadPriority");
            }
        }

        private void TlbFlushThresholdToggle_Click(object sender, RoutedEventArgs e)
        {
            if (TlbFlushThresholdToggle.IsChecked == true)
            {
                // Update both Mm and non-Mm registry values
                UpdateRegistryValue("TlbFlushThreshold", 0x400);  // 0x400 = dword:00000400
                UpdateRegistryValue("MmTlbFlushThreshold", 0x400);
            }
            else
            {
                // Delete both Mm and non-Mm registry values
                DeleteRegistryValue("TlbFlushThreshold");
                DeleteRegistryValue("MmTlbFlushThreshold");
            }
        }

        private void EnforceCachePartitioningToggle_Click(object sender, RoutedEventArgs e)
        {
            if (EnforceCachePartitioningToggle.IsChecked == true)
            {
                // Update both Mm and non-Mm registry values
                UpdateRegistryValue("EnforceCachePartitioning", 1);
                UpdateRegistryValue("MmEnforceCachePartitioning", 1);
            }
            else
            {
                // Delete both Mm and non-Mm registry values
                DeleteRegistryValue("EnforceCachePartitioning");
                DeleteRegistryValue("MmEnforceCachePartitioning");
            }
        }

        private void InvalidateTlbOnForkToggle_Click(object sender, RoutedEventArgs e)
        {
            if (InvalidateTlbOnForkToggle.IsChecked == true)
            {
                // Update both Mm and non-Mm registry values
                UpdateRegistryValue("InvalidateTlbOnFork", 1);
                UpdateRegistryValue("MmInvalidateTlbOnFork", 1);
            }
            else
            {
                // Delete both Mm and non-Mm registry values
                DeleteRegistryValue("InvalidateTlbOnFork");
                DeleteRegistryValue("MmInvalidateTlbOnFork");
            }
        }




        private void LargeSystemCacheToggle_Click(object sender, RoutedEventArgs e)
        {
            if (LargeSystemCacheToggle.IsChecked == true)
            {
                UpdateRegistryValue("MmLargeSystemCache", 0);
                UpdateRegistryValue("LargeSystemCache", 0);// Set to tweaked value
            }
            else
            {
                DeleteRegistryValue("MmLargeSystemCache");
                DeleteRegistryValue("LargeSystemCache");// Delete if toggled off (reset to original)
            }
        }

        private void HotPageThresholdToggle_Click(object sender, RoutedEventArgs e)
        {
            if (HotPageThresholdToggle.IsChecked == true)
            {
                UpdateRegistryValue("HotPageThreshold", 0x1000);
                UpdateRegistryValue("MmHotPageThreshold", 0x1000);
            }
            else
            {
                DeleteRegistryValue("HotPageThreshold");
                DeleteRegistryValue("MmHotPageThreshold");
            }
        }

        private void DirectCacheFlushToggle_Click(object sender, RoutedEventArgs e)
        {
            if (DirectCacheFlushToggle.IsChecked == true)
            {
                UpdateRegistryValue("DirectCacheFlush", 0);
                UpdateRegistryValue("MmDirectCacheFlush", 0);
            }
            else
            {
                DeleteRegistryValue("DirectCacheFlush");
                DeleteRegistryValue("MmDirectCacheFlush");
            }
        }

        private void InterruptDrivenPagingToggle_Click(object sender, RoutedEventArgs e)
        {
            if (InterruptDrivenPagingToggle.IsChecked == true)
            {
                UpdateRegistryValue("InterruptDrivenPaging", 1);
                UpdateRegistryValue("MmInterruptDrivenPaging", 1);
            }
            else
            {
                DeleteRegistryValue("InterruptDrivenPaging");
                DeleteRegistryValue("MmInterruptDrivenPaging");
            }
        }

        private void CompressedStoreThresholdToggle_Click(object sender, RoutedEventArgs e)
        {
            if (CompressedStoreThresholdToggle.IsChecked == true)
            {
                UpdateRegistryValue("CompressedStoreThreshold", 1);
                UpdateRegistryValue("MmCompressedStoreThreshold", 1);
            }
            else
            {
                DeleteRegistryValue("CompressedStoreThreshold");
                DeleteRegistryValue("MmCompressedStoreThreshold");
            }
        }

        private void CacheLinePrefetchToggle_Click(object sender, RoutedEventArgs e)
        {
            if (CacheLinePrefetchToggle.IsChecked == true)
            {
                UpdateRegistryValue("CacheLinePrefetch", 0x03);
                UpdateRegistryValue("MmCacheLinePrefetch", 0x03);
            }
            else
            {
                DeleteRegistryValue("CacheLinePrefetch");
                DeleteRegistryValue("MmCacheLinePrefetch");
            }
        }

        private void PageColorPolicyToggle_Click(object sender, RoutedEventArgs e)
        {
            if (PageColorPolicyToggle.IsChecked == true)
            {
                UpdateRegistryValue("PageColorPolicy", 0x02);
                UpdateRegistryValue("MmPageColorPolicy", 0x02);
            }
            else
            {
                DeleteRegistryValue("PageColorPolicy");
                DeleteRegistryValue("MmPageColorPolicy");
            }
        }

        private void IoMmuBypassToggle_Click(object sender, RoutedEventArgs e)
        {
            if (IoMmuBypassToggle.IsChecked == true)
            {
                UpdateRegistryValue("IoMmuBypass", 1);
                UpdateRegistryValue("MmIoMmuBypass", 1);
            }
            else
            {
                DeleteRegistryValue("IoMmuBypass");
                DeleteRegistryValue("MmIoMmuBypass");
            }
        }

        private void TlbShootdownCoalesceToggle_Click(object sender, RoutedEventArgs e)
        {
            if (TlbShootdownCoalesceToggle.IsChecked == true)
            {
                UpdateRegistryValue("TlbShootdownCoalesce", 0x50);
                UpdateRegistryValue("MmTlbShootdownCoalesce", 0x50);
            }
            else
            {
                DeleteRegistryValue("TlbShootdownCoalesce");
                DeleteRegistryValue("MmTlbShootdownCoalesce");
            }
        }

        private void DynamicPtePromotionToggle_Click(object sender, RoutedEventArgs e)
        {
            if (DynamicPtePromotionToggle.IsChecked == true)
            {
                UpdateRegistryValue("DynamicPtePromotion", 1);
                UpdateRegistryValue("MmDynamicPtePromotion", 1);
            }
            else
            {
                DeleteRegistryValue("DynamicPtePromotion");
                DeleteRegistryValue("MmDynamicPtePromotion");
            }
        }

        private void HardFaultBurstLimitToggle_Click(object sender, RoutedEventArgs e)
        {
            if (HardFaultBurstLimitToggle.IsChecked == true)
            {
                UpdateRegistryValue("HardFaultBurstLimit", 0);
                UpdateRegistryValue("MmHardFaultBurstLimit", 0);
            }
            else
            {
                DeleteRegistryValue("HardFaultBurstLimit");
                DeleteRegistryValue("MmHardFaultBurstLimit");
            }
        }

        private void ResidentAvailableThresholdToggle_Click(object sender, RoutedEventArgs e)
        {
            if (ResidentAvailableThresholdToggle.IsChecked == true)
            {
                UpdateRegistryValue("ResidentAvailableThreshold", 0x90);
                UpdateRegistryValue("MmResidentAvailableThreshold", 0x90);
            }
            else
            {
                DeleteRegistryValue("ResidentAvailableThreshold");
                DeleteRegistryValue("MmResidentAvailableThreshold");
            }
        }

        private void CacheAllocAlignmentToggle_Click(object sender, RoutedEventArgs e)
        {
            if (CacheAllocAlignmentToggle.IsChecked == true)
            {
                UpdateRegistryValue("CacheAllocAlignment", 0x40);
                UpdateRegistryValue("MmCacheAllocAlignment", 0x40);
            }
            else
            {
                DeleteRegistryValue("CacheAllocAlignment");
                DeleteRegistryValue("MmCacheAllocAlignment");
            }
        }

        private void PrefetchDistanceToggle_Click(object sender, RoutedEventArgs e)
        {
            if (PrefetchDistanceToggle.IsChecked == true)
            {
                UpdateRegistryValue("PrefetchDistance", 0x04);
                UpdateRegistryValue("MmPrefetchDistance", 0x04);
            }
            else
            {
                DeleteRegistryValue("PrefetchDistance");
                DeleteRegistryValue("MmPrefetchDistance");
            }
        }

        private void StaleTlbThresholdToggle_Click(object sender, RoutedEventArgs e)
        {
            if (StaleTlbThresholdToggle.IsChecked == true)
            {
                UpdateRegistryValue("StaleTlbThreshold", 0x400);
                UpdateRegistryValue("MmStaleTlbThreshold", 0x400);
            }
            else
            {
                DeleteRegistryValue("StaleTlbThreshold");
                DeleteRegistryValue("MmStaleTlbThreshold");
            }
        }

        private void GpuApertureSizeToggle_Click(object sender, RoutedEventArgs e)
        {
            if (GpuApertureSizeToggle.IsChecked == true)
            {
                UpdateRegistryValue("GpuApertureSize", 0x8000);
                UpdateRegistryValue("MmGpuApertureSize", 0x8000);
            }
            else
            {
                DeleteRegistryValue("GpuApertureSize");
                DeleteRegistryValue("MmGpuApertureSize");
            }
        }

        private void WriteCombineGranularityToggle_Click(object sender, RoutedEventArgs e)
        {
            if (WriteCombineGranularityToggle.IsChecked == true)
            {
                UpdateRegistryValue("WriteCombineGranularity", 0x200);
                UpdateRegistryValue("MmWriteCombineGranularity", 0x200);
            }
            else
            {
                DeleteRegistryValue("WriteCombineGranularity");
                DeleteRegistryValue("MmWriteCombineGranularity");
            }
        }

        private void DisableSmapToggle_Click(object sender, RoutedEventArgs e)
        {
            if (DisableSmapToggle.IsChecked == true)
            {
                UpdateRegistryValue("DisableSmap", 1);
                UpdateRegistryValue("MmDisableSmap", 1);
            }
            else
            {
                DeleteRegistryValue("DisableSmap");
                DeleteRegistryValue("MmDisableSmap");
            }
        }

        private void KernelExecutePoolToggle_Click(object sender, RoutedEventArgs e)
        {
            if (KernelExecutePoolToggle.IsChecked == true)
            {
                UpdateRegistryValue("KernelExecutePool", 0);
                UpdateRegistryValue("MmKernelExecutePool", 0);
            }
            else
            {
                DeleteRegistryValue("KernelExecutePool");
                DeleteRegistryValue("MmKernelExecutePool");
            }
        }

        private void NullDereferenceProtectionToggle_Click(object sender, RoutedEventArgs e)
        {
            if (NullDereferenceProtectionToggle.IsChecked == true)
            {
                UpdateRegistryValue("NullDereferenceProtection", 0);
                UpdateRegistryValue("MmNullDereferenceProtection", 0);
            }
            else
            {
                DeleteRegistryValue("NullDereferenceProtection");
                DeleteRegistryValue("MmNullDereferenceProtection");
            }
        }





        private void LockPagesInMemoryToggle_Click(object sender, RoutedEventArgs e)
        {
            if (LockPagesInMemoryToggle.IsChecked == true)
            {
                UpdateRegistryValue("LockPagesInMemory", 1);
            }
            else
            {
                DeleteRegistryValue("LockPagesInMemory");
            }
        }

        private void LargePageHeapSizeThresholdToggle_Click(object sender, RoutedEventArgs e)
        {
            if (LargePageHeapSizeThresholdToggle.IsChecked == true)
            {
                UpdateRegistryValue("LargePageHeapSizeThreshold", 3);
            }
            else
            {
                DeleteRegistryValue("LargePageHeapSizeThreshold");
            }
        }

        private void UseBiasedLockingToggle_Click(object sender, RoutedEventArgs e)
        {
            if (UseBiasedLockingToggle.IsChecked == true)
            {
                UpdateRegistryValue("UseBiasedLocking", 1);
            }
            else
            {
                DeleteRegistryValue("UseBiasedLocking");
            }
        }

        private void TieredCompilationToggle_Click(object sender, RoutedEventArgs e)
        {
            if (TieredCompilationToggle.IsChecked == true)
            {
                UpdateRegistryValue("TieredCompilation", 16);
            }
            else
            {
                DeleteRegistryValue("TieredCompilation");
            }
        }

        private void TieredStopAtLevelToggle_Click(object sender, RoutedEventArgs e)
        {
            if (TieredStopAtLevelToggle.IsChecked == true)
            {
                UpdateRegistryValue("TieredStopAtLevel", 1);
            }
            else
            {
                DeleteRegistryValue("TieredStopAtLevel");
            }
        }

        private void ThreadStackSizeToggle_Click(object sender, RoutedEventArgs e)
        {
            if (ThreadStackSizeToggle.IsChecked == true)
            {
                UpdateRegistryValue("ThreadStackSize", 3);
            }
            else
            {
                DeleteRegistryValue("ThreadStackSize");
            }
        }


        private void UpdateRegistryValue(string valueName, int value)
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(MemoryKeyPath, writable: true))
                {
                    if (key != null)
                    {
                        key.SetValue(valueName, value, RegistryValueKind.DWord);
                        mainWindow?.MarkSettingsApplied();
                        App.changelogUserControl?.AddLog("Applied", $"{valueName} set to {value}.");
                    }
                    else
                    {
                        ShowError($"Failed to access registry key: {valueName}");
                        App.changelogUserControl?.AddLog("Failed", $"Failed to access registry key: {valueName}");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                ShowError("You do not have permission to modify the registry. Please run the application as an administrator.");
                App.changelogUserControl?.AddLog("Failed", $"Unauthorized access while modifying {valueName}.");
            }
            catch (Exception ex)
            {
                ShowError($"Error updating {valueName}: {ex.Message}");
                App.changelogUserControl?.AddLog("Failed", $"Error updating {valueName}: {ex.Message}");
            }
        }

        private void DeleteRegistryValue(string valueName)
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(MemoryKeyPath, writable: true))
                {
                    if (key != null)
                    {
                        key.DeleteValue(valueName, false);
                        mainWindow?.MarkSettingsApplied();
                        App.changelogUserControl?.AddLog("Applied", $"{valueName} deleted.");
                    }
                    else
                    {
                        ShowError($"Failed to access registry key: {valueName}");
                        App.changelogUserControl?.AddLog("Failed", $"Failed to access registry key: {valueName}");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                ShowError("You do not have permission to modify the registry. Please run the application as an administrator.");
                App.changelogUserControl?.AddLog("Failed", $"Unauthorized access while deleting {valueName}.");
            }
            catch (Exception ex)
            {
                ShowError($"Error deleting {valueName}: {ex.Message}");
                App.changelogUserControl?.AddLog("Failed", $"Error deleting {valueName}: {ex.Message}");
            }
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
