/*
 * Copyright (C) 2017 The Android Open Source Project
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License
 */

package com.android.tools.build.bundletool.model;

import static com.android.tools.build.bundletool.model.AndroidManifest.APP_COMPONENT_FACTORY_ATTRIBUTE_NAME;
import static com.android.tools.build.bundletool.model.AndroidManifest.APP_COMPONENT_FACTORY_RESOURCE_ID;
import static com.android.tools.build.bundletool.model.AndroidManifest.AUTHORITIES_ATTRIBUTE_NAME;
import static com.android.tools.build.bundletool.model.AndroidManifest.AUTHORITIES_RESOURCE_ID;
import static com.android.tools.build.bundletool.model.AndroidManifest.DEBUGGABLE_RESOURCE_ID;
import static com.android.tools.build.bundletool.model.AndroidManifest.DESCRIPTION_ATTRIBUTE_NAME;
import static com.android.tools.build.bundletool.model.AndroidManifest.DESCRIPTION_RESOURCE_ID;
import static com.android.tools.build.bundletool.model.AndroidManifest.DEVELOPMENT_SDK_VERSION;
import static com.android.tools.build.bundletool.model.AndroidManifest.HAS_CODE_RESOURCE_ID;
import static com.android.tools.build.bundletool.model.AndroidManifest.ICON_ATTRIBUTE_NAME;
import static com.android.tools.build.bundletool.model.AndroidManifest.ICON_RESOURCE_ID;
import static com.android.tools.build.bundletool.model.AndroidManifest.INSTALL_LOCATION_RESOURCE_ID;
import static com.android.tools.build.bundletool.model.AndroidManifest.IS_FEATURE_SPLIT_RESOURCE_ID;
import static com.android.tools.build.bundletool.model.AndroidManifest.LABEL_ATTRIBUTE_NAME;
import static com.android.tools.build.bundletool.model.AndroidManifest.LABEL_RESOURCE_ID;
import static com.android.tools.build.bundletool.model.AndroidManifest.LOCALE_CONFIG_ATTRIBUTE_NAME;
import static com.android.tools.build.bundletool.model.AndroidManifest.LOCALE_CONFIG_RESOURCE_ID;
import static com.android.tools.build.bundletool.model.AndroidManifest.MODULE_TYPE_AI_VALUE;
import static com.android.tools.build.bundletool.model.AndroidManifest.MODULE_TYPE_ASSET_VALUE;
import static com.android.tools.build.bundletool.model.AndroidManifest.MODULE_TYPE_FEATURE_VALUE;
import static com.android.tools.build.bundletool.model.AndroidManifest.NAME_ATTRIBUTE_NAME;
import static com.android.tools.build.bundletool.model.AndroidManifest.NAME_RESOURCE_ID;
import static com.android.tools.build.bundletool.model.AndroidManifest.PERMISSION_ELEMENT_NAME;
import static com.android.tools.build.bundletool.model.AndroidManifest.PERMISSION_GROUP_ELEMENT_NAME;
import static com.android.tools.build.bundletool.model.AndroidManifest.RESOURCE_RESOURCE_ID;
import static com.android.tools.build.bundletool.model.AndroidManifest.VALUE_RESOURCE_ID;
import static com.android.tools.build.bundletool.model.AndroidManifest.VERSION_CODE_RESOURCE_ID;
import static com.android.tools.build.bundletool.model.AndroidManifest.VERSION_NAME_RESOURCE_ID;
import static com.android.tools.build.bundletool.model.BundleModule.ModuleType.ASSET_MODULE;
import static com.android.tools.build.bundletool.model.BundleModule.ModuleType.FEATURE_MODULE;
import static com.android.tools.build.bundletool.model.BundleModule.ModuleType.ML_MODULE;
import static com.android.tools.build.bundletool.model.ModuleDeliveryType.ALWAYS_INITIAL_INSTALL;
import static com.android.tools.build.bundletool.model.ModuleDeliveryType.NO_INITIAL_INSTALL;
import static com.android.tools.build.bundletool.testing.ManifestProtoUtils.androidManifest;
import static com.android.tools.build.bundletool.testing.ManifestProtoUtils.androidManifestForAssetModule;
import static com.android.tools.build.bundletool.testing.ManifestProtoUtils.androidManifestForMlModule;
import static com.android.tools.build.bundletool.testing.ManifestProtoUtils.createIntentFilterForMainActivity;
import static com.android.tools.build.bundletool.testing.ManifestProtoUtils.withActivityAlias;
import static com.android.tools.build.bundletool.testing.ManifestProtoUtils.withCustomThemeActivity;
import static com.android.tools.build.bundletool.testing.ManifestProtoUtils.withFastFollowDelivery;
import static com.android.tools.build.bundletool.testing.ManifestProtoUtils.withFeatureCondition;
import static com.android.tools.build.bundletool.testing.ManifestProtoUtils.withFusingAttribute;
import static com.android.tools.build.bundletool.testing.ManifestProtoUtils.withInstallTimeDelivery;
import static com.android.tools.build.bundletool.testing.ManifestProtoUtils.withInstallTimeRemovableElement;
import static com.android.tools.build.bundletool.testing.ManifestProtoUtils.withInstant;
import static com.android.tools.build.bundletool.testing.ManifestProtoUtils.withInstantInstallTimeDelivery;
import static com.android.tools.build.bundletool.testing.ManifestProtoUtils.withInstantOnDemandDelivery;
import static com.android.tools.build.bundletool.testing.ManifestProtoUtils.withLegacyFusingAttribute;
import static com.android.tools.build.bundletool.testing.ManifestProtoUtils.withLegacyOnDemand;
import static com.android.tools.build.bundletool.testing.ManifestProtoUtils.withMainActivity;
import static com.android.tools.build.bundletool.testing.ManifestProtoUtils.withMainTvActivity;
import static com.android.tools.build.bundletool.testing.ManifestProtoUtils.withMaxSdkCondition;
import static com.android.tools.build.bundletool.testing.ManifestProtoUtils.withMinSdkCondition;
import static com.android.tools.build.bundletool.testing.ManifestProtoUtils.withMinSdkVersion;
import static com.android.tools.build.bundletool.testing.ManifestProtoUtils.withNativeActivity;
import static com.android.tools.build.bundletool.testing.ManifestProtoUtils.withOnDemandAttribute;
import static com.android.tools.build.bundletool.testing.ManifestProtoUtils.withOnDemandDelivery;
import static com.android.tools.build.bundletool.testing.ManifestProtoUtils.withSharedUserId;
import static com.android.tools.build.bundletool.testing.ManifestProtoUtils.withTargetSandboxVersion;
import static com.android.tools.build.bundletool.testing.ManifestProtoUtils.withTypeAttribute;
import static com.android.tools.build.bundletool.testing.ManifestProtoUtils.withUsesSplit;
import static com.android.tools.build.bundletool.testing.ManifestProtoUtils.xmlAttribute;
import static com.android.tools.build.bundletool.testing.ManifestProtoUtils.xmlBooleanAttribute;
import static com.android.tools.build.bundletool.testing.ManifestProtoUtils.xmlDecimalIntegerAttribute;
import static com.android.tools.build.bundletool.testing.ManifestProtoUtils.xmlElement;
import static com.android.tools.build.bundletool.testing.ManifestProtoUtils.xmlNamespace;
import static com.android.tools.build.bundletool.testing.ManifestProtoUtils.xmlNode;
import static com.android.tools.build.bundletool.testing.ManifestProtoUtils.xmlResourceReferenceAttribute;
import static com.google.common.truth.Truth.assertThat;
import static com.google.common.truth.extensions.proto.ProtoTruth.assertThat;
import static org.junit.jupiter.api.Assertions.assertThrows;

import com.android.aapt.Resources.XmlElement;
import com.android.aapt.Resources.XmlNode;
import com.android.tools.build.bundletool.TestData;
import com.android.tools.build.bundletool.model.exceptions.InvalidBundleException;
import com.android.tools.build.bundletool.model.utils.xmlproto.UnexpectedAttributeTypeException;
import com.android.tools.build.bundletool.model.utils.xmlproto.XmlProtoElement;
import com.android.tools.build.bundletool.model.utils.xmlproto.XmlProtoNode;
import com.android.tools.build.bundletool.model.version.Version;
import com.google.common.collect.ImmutableList;
import com.google.protobuf.TextFormat;
import java.util.Optional;
import org.junit.Test;
import org.junit.experimental.theories.DataPoints;
import org.junit.experimental.theories.FromDataPoints;
import org.junit.experimental.theories.Theories;
import org.junit.experimental.theories.Theory;
import org.junit.runner.RunWith;

/** Tests for {@link AndroidManifest}. */
@RunWith(Theories.class)
public class AndroidManifestTest {

  private static final String ANDROID_NAMESPACE_URI = "http://schemas.android.com/apk/res/android";
  private static final String DISTRIBUTION_NAMESPACE_URI =
      "http://schemas.android.com/apk/distribution";

  private static final Version BUNDLE_TOOL_0_3_4 = Version.of("0.3.4");
  private static final Version BUNDLE_TOOL_0_3_3 = Version.of("0.3.3");

  @DataPoints("sdkCodenames")
  public static final String[] ANDROID_SDK_CODENAMES = {"R", "Q", "Sv2", "Tiramisu"};

  @Test
  public void getApplicationDebuggable_absent() {
    AndroidManifest androidManifest =
        AndroidManifest.create(xmlNode(xmlElement("manifest", xmlNode(xmlElement("application")))));
    assertThat(androidManifest.getApplicationDebuggable()).isEmpty();
    assertThat(androidManifest.getEffectiveApplicationDebuggable()).isFalse();
  }

  @Test
  public void hasMainActivity_definedAsActivityAlias_returnTrue() {
    AndroidManifest androidManifest =
        AndroidManifest.create(
            androidManifest(
                "com.test.app",
                withActivityAlias(
                    "com.test.app.MainActivity",
                    activity ->
                        activity
                            .addChildElement(
                                createIntentFilterForMainActivity(
                                    "android.intent.category.LAUNCHER"))
                            .addChildElement(
                                createIntentFilterForMainActivity(
                                    "android.intent.category.LEANBACK_LAUNCHER")))));

    assertThat(androidManifest.hasMainTvActivity()).isTrue();
    assertThat(androidManifest.hasMainActivity()).isTrue();
  }

  @Test
  public void getApplicationDebuggable_presentFalse() {
    AndroidManifest androidManifest =
        AndroidManifest.create(
            xmlNode(
                xmlElement(
                    "manifest",
                    xmlNode(
                        xmlElement(
                            "application",
                            xmlBooleanAttribute(
                                ANDROID_NAMESPACE_URI,
                                "debuggable",
                                DEBUGGABLE_RESOURCE_ID,
                                false))))));
    assertThat(androidManifest.getApplicationDebuggable()).hasValue(false);
    assertThat(androidManifest.getEffectiveApplicationDebuggable()).isFalse();
  }

  @Test
  public void getApplicationDebuggable_presentTrue() {
    AndroidManifest androidManifest =
        AndroidManifest.create(
            xmlNode(
                xmlElement(
                    "manifest",
                    xmlNode(
                        xmlElement(
                            "application",
                            xmlBooleanAttribute(
                                ANDROID_NAMESPACE_URI,
                                "debuggable",
                                DEBUGGABLE_RESOURCE_ID,
                                true))))));
    assertThat(androidManifest.getApplicationDebuggable()).hasValue(true);
    assertThat(androidManifest.getEffectiveApplicationDebuggable()).isTrue();
  }

  // There are tests only for getMinSdkVersion. The getMaxSdkVersion, getTargetSdkVersion methods
  // rely on the same underlying implementation.

  @Test
  public void getMinSdkVersion_positive() {
    AndroidManifest androidManifest =
        AndroidManifest.create(androidManifest("com.test.app", withMinSdkVersion(123)));
    assertThat(androidManifest.getMinSdkVersion()).hasValue(123);
  }

  @Test
  public void getMinSdkVersion_negative() {
    AndroidManifest androidManifest = AndroidManifest.create(androidManifest("com.test.app"));
    assertThat(androidManifest.getMinSdkVersion()).isEmpty();
  }

  @Test
  @Theory
  public void getMinSdkVersion_asString(@FromDataPoints("sdkCodenames") String codename) {
    AndroidManifest androidManifest =
        AndroidManifest.create(androidManifest("com.test.app", withMinSdkVersion(codename)));
    assertThat(androidManifest.getMinSdkVersion()).hasValue(DEVELOPMENT_SDK_VERSION);
  }

  @Test
  public void getMinSdkVersion_asStringWithFingerprint() {
    AndroidManifest androidManifest =
        AndroidManifest.create(androidManifest("com.test.app", withMinSdkVersion("R.fingerprint")));
    assertThat(androidManifest.getMinSdkVersion()).hasValue(DEVELOPMENT_SDK_VERSION);
  }

  @Test
  public void getTargetSandboxVersion_empty() {
    AndroidManifest androidManifest = AndroidManifest.create(androidManifest("com.test.app"));
    assertThat(androidManifest.getTargetSandboxVersion()).isEmpty();
  }

  @Test
  public void getTargetSandboxVersion_setTo2() {
    AndroidManifest androidManifest =
        AndroidManifest.create(androidManifest("com.test.app", withTargetSandboxVersion(2)));
    assertThat(androidManifest.getTargetSandboxVersion()).hasValue(2);
  }

  @Test
  public void getUsesSplits_positive() {
    AndroidManifest androidManifest =
        AndroidManifest.create(androidManifest("com.test.app", withUsesSplit("parent")));
    assertThat(androidManifest.getUsesSplits()).containsExactly("parent");
  }

  @Test
  public void getUsesSplits_negative() {
    AndroidManifest androidManifest = AndroidManifest.create(androidManifest("com.test.app"));
    assertThat(androidManifest.getUsesSplits()).isEmpty();
  }

  @Test
  public void getUsesSplits_missingNameAttribute_throws() {
    AndroidManifest androidManifest =
        AndroidManifest.create(xmlNode(xmlElement("manifest", xmlNode(xmlElement("uses-split")))));
    InvalidBundleException exception =
        assertThrows(InvalidBundleException.class, androidManifest::getUsesSplits);
    assertThat(exception)
        .hasMessageThat()
        .contains("<uses-split> element is missing the 'android:name' attribute");
  }

  @Test
  public void hasSplitId_positive() {
    AndroidManifest androidManifest =
        AndroidManifest.create(
            xmlNode(
                xmlElement(
                    "manifest",
                    xmlAttribute("split", "config.x86"),
                    xmlNode(xmlElement("application")))));
    assertThat(androidManifest.getSplitId()).hasValue("config.x86");
  }

  @Test
  public void hasCode_negative() {
    AndroidManifest androidManifest =
        AndroidManifest.create(
            xmlNode(
                xmlElement(
                    "manifest",
                    xmlNode(
                        xmlElement(
                            "application",
                            xmlBooleanAttribute(
                                ANDROID_NAMESPACE_URI, "hasCode", HAS_CODE_RESOURCE_ID, false))))));
    assertThat(androidManifest.getHasCode()).hasValue(false);
  }

  @Test
  public void hasCode_positive() {
    AndroidManifest androidManifest =
        AndroidManifest.create(
            xmlNode(
                xmlElement(
                    "manifest",
                    xmlNode(
                        xmlElement(
                            "application",
                            xmlBooleanAttribute(
                                ANDROID_NAMESPACE_URI, "hasCode", HAS_CODE_RESOURCE_ID, true))))));
    assertThat(androidManifest.getHasCode()).hasValue(true);
  }

  @Test
  public void hasCode_emptyMeansTrue() {
    AndroidManifest androidManifest =
        AndroidManifest.create(xmlNode(xmlElement("manifest", xmlNode(xmlElement("application")))));
    assertThat(androidManifest.getHasCode()).isEmpty();
    assertThat(androidManifest.getEffectiveHasCode()).isTrue();
  }

  @Test
  public void isFeatureSplit_negative() {
    AndroidManifest androidManifest =
        AndroidManifest.create(
            xmlNode(
                xmlElement(
                    "manifest",
                    xmlBooleanAttribute(
                        ANDROID_NAMESPACE_URI,
                        "isFeatureSplit",
                        IS_FEATURE_SPLIT_RESOURCE_ID,
                        false),
                    xmlNode(xmlElement("application")))));
    assertThat(androidManifest.getIsFeatureSplit()).hasValue(false);
  }

  @Test
  public void isFeatureSplit_positive() {
    AndroidManifest androidManifest =
        AndroidManifest.create(
            xmlNode(
                xmlElement(
                    "manifest",
                    xmlBooleanAttribute(
                        ANDROID_NAMESPACE_URI,
                        "isFeatureSplit",
                        IS_FEATURE_SPLIT_RESOURCE_ID,
                        true),
                    xmlNode(xmlElement("application")))));
    assertThat(androidManifest.getIsFeatureSplit()).hasValue(true);
  }

  @Test
  public void isFeatureSplit_empty() {
    AndroidManifest androidManifest =
        AndroidManifest.create(xmlNode(xmlElement("manifest", xmlNode(xmlElement("application")))));
    assertThat(androidManifest.getIsFeatureSplit()).isEmpty();
  }

  @Test
  public void getFeatureSplitId_empty() {
    AndroidManifest androidManifest =
        AndroidManifest.create(xmlNode(xmlElement("manifest", xmlNode(xmlElement("application")))));
    assertThat(androidManifest.getConfigForSplit()).isEmpty();
  }

  @Test
  public void getFeatureSplitId_present() {
    AndroidManifest androidManifest =
        AndroidManifest.create(
            xmlNode(
                xmlElement(
                    "manifest",
                    xmlAttribute("configForSplit", "feature1"),
                    xmlNode(xmlElement("application")))));
    assertThat(androidManifest.getConfigForSplit()).hasValue("feature1");
  }

  @Test
  public void getPackageName() {
    AndroidManifest androidManifest =
        AndroidManifest.create(
            xmlNode(
                xmlElement(
                    "manifest",
                    xmlAttribute("package", "com.test.app"),
                    xmlNode(xmlElement("application")))));
    assertThat(androidManifest.getPackageName()).isEqualTo("com.test.app");
  }

  @Test
  public void getVersionCode() {
    AndroidManifest androidManifest =
        AndroidManifest.create(
            xmlNode(
                xmlElement(
                    "manifest",
                    xmlDecimalIntegerAttribute(
                        ANDROID_NAMESPACE_URI, "versionCode", VERSION_CODE_RESOURCE_ID, 123),
                    xmlNode(xmlElement("application")))));
    assertThat(androidManifest.getVersionCode()).hasValue(123);
  }

  @Test
  public void getInstallLocation() {
    AndroidManifest androidManifest =
        AndroidManifest.create(
            xmlNode(
                xmlElement(
                    "manifest",
                    xmlDecimalIntegerAttribute(
                        ANDROID_NAMESPACE_URI, "installLocation", INSTALL_LOCATION_RESOURCE_ID, 0),
                    xmlNode(xmlElement("application")))));
    assertThat(androidManifest.getInstallLocationValue()).hasValue("auto");
  }

  @Test
  public void getVersionCode_missing_isEmpty() {
    AndroidManifest androidManifest =
        AndroidManifest.create(xmlNode(xmlElement("manifest", xmlNode(xmlElement("application")))));
    assertThat(androidManifest.getVersionCode()).isEmpty();
  }

  @Test
  public void getVersionCode_invalid_throws() {
    AndroidManifest androidManifest =
        AndroidManifest.create(
            xmlNode(
                xmlElement(
                    "manifest",
                    xmlAttribute(
                        ANDROID_NAMESPACE_URI, "versionCode", VERSION_CODE_RESOURCE_ID, "bad!"),
                    xmlNode(xmlElement("application")))));
    UnexpectedAttributeTypeException exception =
        assertThrows(
            UnexpectedAttributeTypeException.class, () -> androidManifest.getVersionCode());

    assertThat(exception)
        .hasMessageThat()
        .contains("Attribute 'versionCode' expected to have type 'decimal int' but found:");
  }

  @Test
  public void getVersionName() {
    AndroidManifest androidManifest =
        AndroidManifest.create(
            xmlNode(
                xmlElement(
                    "manifest",
                    xmlAttribute(
                        ANDROID_NAMESPACE_URI,
                        "versionName",
                        VERSION_NAME_RESOURCE_ID,
                        "new app version"),
                    xmlNode(xmlElement("application")))));
    assertThat(androidManifest.getVersionName()).hasValue("new app version");
  }

  @Test
  public void getVersionName_missing_isEmpty() {
    AndroidManifest androidManifest =
        AndroidManifest.create(xmlNode(xmlElement("manifest", xmlNode(xmlElement("application")))));
    assertThat(androidManifest.getVersionName()).isEmpty();
  }

  @Test
  public void hasSplitId_negative() {
    AndroidManifest androidManifest =
        AndroidManifest.create(xmlNode(xmlElement("manifest", xmlNode(xmlElement("application")))));
    assertThat(androidManifest.getSplitId()).isEmpty();
  }

  @Test
  public void deliveryTypeAndOnDemandAttribute_notSet() {
    // From Bundletool 0.3.4 onwards.
    AndroidManifest manifest =
        AndroidManifest.create(androidManifest("com.test.app"), BUNDLE_TOOL_0_3_4);
    assertThat(manifest.getManifestDeliveryElement()).isEmpty();
    assertThat(manifest.getOnDemandAttribute()).isEmpty();
    assertThat(manifest.isDeliveryTypeDeclared()).isFalse();
  }

  @Test
  public void deliveryTypeAndOnDemandAttribute_notSet_legacy() {
    AndroidManifest manifest =
        AndroidManifest.create(androidManifest("com.test.app"), BUNDLE_TOOL_0_3_3);
    assertThat(manifest.getManifestDeliveryElement()).isEmpty();
    assertThat(manifest.getOnDemandAttribute()).isEmpty();
    assertThat(manifest.isDeliveryTypeDeclared()).isFalse();
  }

  @Test
  public void deliveryTypeAndOnDemandAttribute_true() {
    // From Bundletool 0.3.4 onwards.
    AndroidManifest manifest =
        AndroidManifest.create(
            androidManifest("com.test.app", withOnDemandAttribute(true)), BUNDLE_TOOL_0_3_4);
    assertThat(manifest.getManifestDeliveryElement()).isEmpty();
    assertThat(manifest.getOnDemandAttribute()).isPresent();
    assertThat(manifest.isDeliveryTypeDeclared()).isTrue();
  }

  @Test
  public void deliveryTypeAndOnDemandAttribute_true_legacy() {
    AndroidManifest manifest =
        AndroidManifest.create(
            androidManifest("com.test.app", withLegacyOnDemand(true)), BUNDLE_TOOL_0_3_3);
    assertThat(manifest.getManifestDeliveryElement()).isEmpty();
    assertThat(manifest.getOnDemandAttribute()).isPresent();
    assertThat(manifest.isDeliveryTypeDeclared()).isTrue();

    AndroidManifest newManifest =
        AndroidManifest.create(
            androidManifest("com.test.app", withOnDemandAttribute(true)), BUNDLE_TOOL_0_3_3);
    assertThat(newManifest.getManifestDeliveryElement()).isEmpty();
    assertThat(newManifest.getOnDemandAttribute()).isPresent();
    assertThat(newManifest.isDeliveryTypeDeclared()).isTrue();
  }

  @Test
  public void deliveryTypeAndOnDemandAttribute_false() {
    // From Bundletool 0.3.4 onwards.
    AndroidManifest manifest =
        AndroidManifest.create(
            androidManifest("com.test.app", withOnDemandAttribute(false)), BUNDLE_TOOL_0_3_4);
    assertThat(manifest.getManifestDeliveryElement()).isEmpty();
    assertThat(manifest.getOnDemandAttribute()).isPresent();
    assertThat(manifest.isDeliveryTypeDeclared()).isTrue();
  }

  @Test
  public void deliveryTypeAndOnDemandAttribute_false_legacy() {
    AndroidManifest manifest =
        AndroidManifest.create(
            androidManifest("com.test.app", withLegacyOnDemand(false)), BUNDLE_TOOL_0_3_3);
    assertThat(manifest.getManifestDeliveryElement()).isEmpty();
    assertThat(manifest.getOnDemandAttribute()).isPresent();
    assertThat(manifest.isDeliveryTypeDeclared()).isTrue();

    AndroidManifest newManifest =
        AndroidManifest.create(
            androidManifest("com.test.app", withOnDemandAttribute(false)), BUNDLE_TOOL_0_3_3);
    assertThat(newManifest.getManifestDeliveryElement()).isEmpty();
    assertThat(newManifest.getOnDemandAttribute()).isPresent();
    assertThat(newManifest.isDeliveryTypeDeclared()).isTrue();
  }

  @Test
  public void deliveryTypeAndOnDemandAttribute_old_namespace_returnsEmpty() {
    // From Bundletool 0.3.4 onwards.
    AndroidManifest manifest =
        AndroidManifest.create(
            androidManifest("com.test.app", withLegacyOnDemand(false)), BUNDLE_TOOL_0_3_4);
    assertThat(manifest.getManifestDeliveryElement()).isEmpty();
    assertThat(manifest.getOnDemandAttribute()).isEmpty();
    assertThat(manifest.isDeliveryTypeDeclared()).isFalse();
  }

  @Test
  public void deliveryTypeAndOnDemandAttribute_deliveryElement_installTime() {
    AndroidManifest manifest =
        AndroidManifest.create(androidManifest("com.test.app", withInstallTimeDelivery()));

    assertThat(manifest.getManifestDeliveryElement()).isPresent();
    assertThat(manifest.getOnDemandAttribute()).isEmpty();
    assertThat(manifest.isDeliveryTypeDeclared()).isTrue();
  }

  @Test
  public void deliveryTypeAndOnDemandAttribute_deliveryElement_onDemand() {
    AndroidManifest manifest =
        AndroidManifest.create(androidManifest("com.test.app", withOnDemandDelivery()));

    assertThat(manifest.getManifestDeliveryElement()).isPresent();
    assertThat(manifest.getOnDemandAttribute()).isEmpty();
    assertThat(manifest.isDeliveryTypeDeclared()).isTrue();
  }

  @Test
  public void deliveryTypeAndOnDemandAttribute_deliveryElement_fastFollow() {
    AndroidManifest manifest =
        AndroidManifest.create(
            androidManifest(
                "com.test.app",
                withTypeAttribute(MODULE_TYPE_ASSET_VALUE),
                withFastFollowDelivery()));

    assertThat(manifest.getManifestDeliveryElement()).isPresent();
    assertThat(manifest.getOnDemandAttribute()).isEmpty();
    assertThat(manifest.isDeliveryTypeDeclared()).isTrue();
  }

  @Test
  public void deliveryTypeAndOnDemandAttribute_deliveryElement_minSdkCondition() {
    AndroidManifest manifest =
        AndroidManifest.create(androidManifest("com.test.app", withMinSdkCondition(21)));

    assertThat(manifest.getManifestDeliveryElement()).isPresent();
    assertThat(manifest.getOnDemandAttribute()).isEmpty();
    assertThat(manifest.isDeliveryTypeDeclared()).isTrue();
  }

  @Test
  public void deliveryTypeAndOnDemandAttribute_deliveryElement_maxSdkCondition() {
    AndroidManifest manifest =
        AndroidManifest.create(androidManifest("com.test.app", withMaxSdkCondition(21)));

    assertThat(manifest.getManifestDeliveryElement()).isPresent();
    assertThat(manifest.getOnDemandAttribute()).isEmpty();
    assertThat(manifest.isDeliveryTypeDeclared()).isTrue();
  }

  @Test
  public void instantDeliveryType_deliveryElement_onDemand() {
    AndroidManifest manifest =
        AndroidManifest.create(
            androidManifestForAssetModule("com.test.app", withInstantOnDemandDelivery()));

    assertThat(manifest.getInstantManifestDeliveryElement()).isPresent();
    assertThat(
            manifest
                .getInstantManifestDeliveryElement()
                .map(ManifestDeliveryElement::hasOnDemandElement))
        .hasValue(true);
    assertThat(manifest.isInstantModule()).hasValue(true);
  }

  @Test
  public void instantDeliveryType_deliveryElement_installTime() {
    AndroidManifest manifest =
        AndroidManifest.create(
            androidManifestForAssetModule("com.test.app", withInstantInstallTimeDelivery()));

    assertThat(manifest.getInstantManifestDeliveryElement()).isPresent();
    assertThat(
            manifest
                .getInstantManifestDeliveryElement()
                .map(ManifestDeliveryElement::hasInstallTimeElement))
        .hasValue(true);
    assertThat(manifest.isInstantModule()).hasValue(true);
  }

  @Test
  public void moduleTypeAttribute_assetModule() {
    AndroidManifest manifest =
        AndroidManifest.create(androidManifestForAssetModule("com.test.app"));

    assertThat(manifest.getOptionalModuleType()).isPresent();
    assertThat(manifest.getOptionalModuleType()).hasValue(ASSET_MODULE);
    assertThat(manifest.getModuleType()).isEqualTo(ASSET_MODULE);
  }

  @Test
  public void moduleTypeAttribute_aiPack() {
    AndroidManifest manifest =
        AndroidManifest.create(
            androidManifestForAssetModule("com.test.app", withTypeAttribute(MODULE_TYPE_AI_VALUE)));

    assertThat(manifest.getOptionalModuleType()).hasValue(ASSET_MODULE);
    assertThat(manifest.getOptionalModuleTypeAttributeValue()).hasValue(MODULE_TYPE_AI_VALUE);
    assertThat(manifest.getModuleType()).isEqualTo(ASSET_MODULE);
  }

  @Test
  public void moduleTypeAttribute_featureModule() {
    AndroidManifest manifest =
        AndroidManifest.create(
            androidManifest("com.test.app", withTypeAttribute(MODULE_TYPE_FEATURE_VALUE)));

    assertThat(manifest.getOptionalModuleType()).isPresent();
    assertThat(manifest.getOptionalModuleType()).hasValue(FEATURE_MODULE);
    assertThat(manifest.getModuleType()).isEqualTo(FEATURE_MODULE);
  }

  @Test
  public void moduleTypeAttribute_mlModule() {
    AndroidManifest manifest = AndroidManifest.create(androidManifestForMlModule("com.test.app"));

    assertThat(manifest.getOptionalModuleType()).isPresent();
    assertThat(manifest.getOptionalModuleType()).hasValue(ML_MODULE);
    assertThat(manifest.getModuleType()).isEqualTo(ML_MODULE);
  }

  @Test
  public void moduleTypeAttribute_defaultsToFeatureModule() {
    AndroidManifest manifest = AndroidManifest.create(androidManifest("com.test.app"));

    assertThat(manifest.getOptionalModuleType()).isEmpty();
    assertThat(manifest.getModuleType()).isEqualTo(FEATURE_MODULE);
  }

  @Test
  public void moduleTypeAttribute_invalid_throws() {
    AndroidManifest manifest =
        AndroidManifest.create(
            androidManifest("com.test.app", withTypeAttribute("invalid-attribute")));

    InvalidBundleException exception =
        assertThrows(InvalidBundleException.class, manifest::getModuleType);
    assertThat(exception)
        .hasMessageThat()
        .contains("Found invalid type attribute invalid-attribute for <module> element.");
  }

  @Test
  public void getIsIncludedInFusing_true() {
    // From Bundletool 0.3.4 onwards.
    AndroidManifest manifest =
        AndroidManifest.create(
            androidManifest("com.test.app", withFusingAttribute(true)), BUNDLE_TOOL_0_3_4);
    assertThat(manifest.getIsModuleIncludedInFusing()).hasValue(true);
  }

  @Test
  public void getIsIncludedInFusing_legacy_true() {
    AndroidManifest manifest =
        AndroidManifest.create(
            androidManifest("com.test.app", withLegacyFusingAttribute(true)), BUNDLE_TOOL_0_3_3);
    assertThat(manifest.getIsModuleIncludedInFusing()).hasValue(true);

    AndroidManifest newManifest =
        AndroidManifest.create(
            androidManifest("com.test.app", withFusingAttribute(true)), BUNDLE_TOOL_0_3_3);
    assertThat(newManifest.getIsModuleIncludedInFusing()).hasValue(true);
  }

  @Test
  public void getIsIncludedInFusing_false() {
    // From Bundletool 0.3.4 onwards.
    AndroidManifest androidManifest =
        AndroidManifest.create(
            androidManifest("com.test.app", withFusingAttribute(false)), BUNDLE_TOOL_0_3_4);
    assertThat(androidManifest.getIsModuleIncludedInFusing()).hasValue(false);
  }

  @Test
  public void getIsIncludedInFusing_no_namespace_throws() {
    // From Bundletool 0.3.4 onwards.
    AndroidManifest androidManifest =
        AndroidManifest.create(
            androidManifest("com.test.app", withLegacyFusingAttribute(false)), BUNDLE_TOOL_0_3_4);
    assertThrows(InvalidBundleException.class, androidManifest::getIsModuleIncludedInFusing);
  }

  @Test
  public void getIsIncludedInFusing_legacy_false() {
    AndroidManifest manifest =
        AndroidManifest.create(
            androidManifest("com.test.app", withLegacyFusingAttribute(false)), BUNDLE_TOOL_0_3_3);
    assertThat(manifest.getIsModuleIncludedInFusing()).hasValue(false);

    AndroidManifest newManifest =
        AndroidManifest.create(
            androidManifest("com.test.app", withFusingAttribute(false)), BUNDLE_TOOL_0_3_3);
    assertThat(newManifest.getIsModuleIncludedInFusing()).hasValue(false);
  }

  @Test
  public void getIsIncludedInFusing_missingElements_emptyOptional() {
    // From Bundletool 0.3.4 onwards.
    AndroidManifest androidManifest =
        AndroidManifest.create(androidManifest("com.test.app"), BUNDLE_TOOL_0_3_4);
    assertThat(androidManifest.getIsModuleIncludedInFusing()).isEmpty();
  }

  @Test
  public void getIsIncludedInFusing_missingElements_legacy_emptyOptional() {
    AndroidManifest androidManifest =
        AndroidManifest.create(androidManifest("com.test.app"), BUNDLE_TOOL_0_3_3);
    assertThat(androidManifest.getIsModuleIncludedInFusing()).isEmpty();
  }

  @Test
  public void getIsIncludedInFusing_missingIncludeAttribute_forBase_throws() {
    // From Bundletool 0.3.4 onwards.
    AndroidManifest androidManifest =
        AndroidManifest.create(
            xmlNode(
                xmlElement(
                    "manifest",
                    xmlNode(
                        xmlElement(
                            DISTRIBUTION_NAMESPACE_URI,
                            "module",
                            xmlNode(xmlElement(DISTRIBUTION_NAMESPACE_URI, "fusing")))))),
            BUNDLE_TOOL_0_3_4);
    InvalidBundleException exception =
        assertThrows(InvalidBundleException.class, androidManifest::getIsModuleIncludedInFusing);
    assertThat(exception)
        .hasMessageThat()
        .contains("<fusing> element is missing the 'include' attribute");
  }

  @Test
  public void getIsIncludedInFusing_missingIncludeAttribute_forBase_legacy_throws() {
    AndroidManifest androidManifest =
        AndroidManifest.create(
            xmlNode(
                xmlElement(
                    "manifest",
                    xmlNode(
                        xmlElement(
                            DISTRIBUTION_NAMESPACE_URI,
                            "module",
                            xmlNode(xmlElement(DISTRIBUTION_NAMESPACE_URI, "fusing")))))),
            BUNDLE_TOOL_0_3_3);
    InvalidBundleException exception =
        assertThrows(InvalidBundleException.class, androidManifest::getIsModuleIncludedInFusing);
    assertThat(exception)
        .hasMessageThat()
        .contains("<fusing> element is missing the 'include' attribute");
  }

  @Test
  public void getIsIncludedInFusing_missingIncludeAttribute_forSplit_throws() {
    // From Bundletool 0.3.4 onwards.
    AndroidManifest androidManifest =
        AndroidManifest.create(
            xmlNode(
                xmlElement(
                    "manifest",
                    xmlAttribute("split", "feature1"),
                    xmlNode(
                        xmlElement(
                            DISTRIBUTION_NAMESPACE_URI,
                            "module",
                            xmlNode(xmlElement(DISTRIBUTION_NAMESPACE_URI, "fusing")))))),
            BUNDLE_TOOL_0_3_4);
    InvalidBundleException exception =
        assertThrows(InvalidBundleException.class, androidManifest::getIsModuleIncludedInFusing);
    assertThat(exception)
        .hasMessageThat()
        .isEqualTo("<fusing> element is missing the 'include' attribute (split: 'feature1').");
  }

  @Test
  public void getIsIncludedInFusing_missingIncludeAttribute_forSplit_legacy_throws() {
    AndroidManifest androidManifest =
        AndroidManifest.create(
            xmlNode(
                xmlElement(
                    "manifest",
                    xmlAttribute("split", "feature1"),
                    xmlNode(
                        xmlElement(
                            DISTRIBUTION_NAMESPACE_URI,
                            "module",
                            xmlNode(xmlElement(DISTRIBUTION_NAMESPACE_URI, "fusing")))))),
            BUNDLE_TOOL_0_3_3);
    InvalidBundleException exception =
        assertThrows(InvalidBundleException.class, androidManifest::getIsModuleIncludedInFusing);
    assertThat(exception)
        .hasMessageThat()
        .isEqualTo("<fusing> element is missing the 'include' attribute (split: 'feature1').");
  }

  @Test
  public void getFusedModuleNames_missing() {
    AndroidManifest androidManifest =
        AndroidManifest.create(
            xmlNode(
                xmlElement(
                    "manifest",
                    xmlNode(
                        xmlElement(
                            "application",
                            xmlNode(
                                xmlElement(
                                    "meta-data",
                                    ImmutableList.of(
                                        xmlNamespace("android", ANDROID_NAMESPACE_URI)),
                                    ImmutableList.of(
                                        xmlAttribute(ANDROID_NAMESPACE_URI, "name", "plain"),
                                        xmlAttribute(ANDROID_NAMESPACE_URI, "value", "v1")))),
                            metadataWithValue("via android:value", "v2"),
                            metadataWithResourceRef("via android:resource", 3))))));

    assertThat(androidManifest.getFusedModuleNames()).isEmpty();
  }

  @Test
  public void getFusedModuleNames_present() {
    AndroidManifest androidManifest =
        AndroidManifest.create(
            xmlNode(
                xmlElement(
                    "manifest",
                    xmlNode(
                        xmlElement(
                            "application",
                            metadataWithValue(
                                "com.android.dynamic.apk.fused.modules", "base,feature"))))));
    assertThat(androidManifest.getFusedModuleNames()).containsExactly("base", "feature");
  }

  @Test
  public void getFusedModuleNames_multiple_throws() {
    AndroidManifest androidManifest =
        AndroidManifest.create(
            xmlNode(
                xmlElement(
                    "manifest",
                    xmlNode(
                        xmlElement(
                            "application",
                            metadataWithValue("com.android.dynamic.apk.fused.modules", "value1"),
                            metadataWithValue(
                                "com.android.dynamic.apk.fused.modules", "value2"))))));
    InvalidBundleException exception =
        assertThrows(InvalidBundleException.class, androidManifest::getFusedModuleNames);
    assertThat(exception).hasMessageThat().contains("multiple <meta-data> elements for key");
  }

  @Test
  public void twoEqualProtos_objectsEqual() {
    AndroidManifest androidManifest1 =
        AndroidManifest.create(
            xmlNode(
                xmlElement(
                    "manifest",
                    xmlAttribute("split", "config.mips"),
                    xmlNode(xmlElement("application")))));

    AndroidManifest androidManifest2 =
        AndroidManifest.create(
            xmlNode(
                xmlElement(
                    "manifest",
                    xmlAttribute("split", "config.mips"),
                    xmlNode(xmlElement("application")))));
    assertThat(androidManifest1).isEqualTo(androidManifest2);
    assertThat(androidManifest1.hashCode()).isEqualTo(androidManifest2.hashCode());
  }

  @Test
  public void twoDifferentProtos_objectsNotEqual() {
    AndroidManifest androidManifest1 =
        AndroidManifest.create(
            xmlNode(
                xmlElement(
                    "manifest",
                    xmlAttribute("split", "config.mips"),
                    xmlNode(xmlElement("application")))));

    AndroidManifest androidManifest2 =
        AndroidManifest.create(
            xmlNode(
                xmlElement(
                    "manifest",
                    xmlAttribute("split", "config.x86"),
                    xmlNode(xmlElement("application")))));
    assertThat(androidManifest1).isNotEqualTo(androidManifest2);
    assertThat(androidManifest1.hashCode()).isNotEqualTo(androidManifest2.hashCode());
  }

  @Test
  public void settingEmptySplitIdThrows() {
    AndroidManifest moduleManifest = AndroidManifest.create(androidManifest("com.package.test"));
    assertThrows(
        IllegalArgumentException.class,
        () ->
            AndroidManifest.createForConfigSplit(
                moduleManifest.getPackageName(),
                moduleManifest.getVersionCode(),
                "",
                "feature1",
                Optional.empty()));
  }

  @Test
  public void configSplitPropertiesSet() {
    AndroidManifest moduleManifest = AndroidManifest.create(androidManifest("com.package.test"));
    AndroidManifest configManifest =
        AndroidManifest.createForConfigSplit(
            moduleManifest.getPackageName(),
            moduleManifest.getVersionCode(),
            "x86",
            "feature1",
            Optional.of(false));

    assertThat(configManifest.getPackageName()).isEqualTo("com.package.test");
    assertThat(configManifest.getVersionCode()).hasValue(1);
    assertThat(configManifest.getHasCode()).hasValue(false);
    assertThat(configManifest.getSplitId()).hasValue("x86");
    assertThat(configManifest.getConfigForSplit()).hasValue("feature1");
    assertThat(configManifest.isDeliveryTypeDeclared()).isFalse();
    assertThat(configManifest.getIsFeatureSplit()).isEmpty();
    assertThat(configManifest.getExtractNativeLibsValue()).hasValue(false);
  }

  @Test
  public void configSplit_emptyFeatureSplitId() {
    AndroidManifest moduleManifest = AndroidManifest.create(androidManifest("com.package.test"));
    AndroidManifest configManifest =
        AndroidManifest.createForConfigSplit(
            moduleManifest.getPackageName(),
            moduleManifest.getVersionCode(),
            /* splitId= */ "x86",
            /* featureSplitId= */ "",
            /* extractNativeLibs= */ Optional.of(false));
    assertThat(configManifest.getConfigForSplit()).isEmpty();
  }

  @Test
  public void configSplit_noExtraElementsFromModuleSplit() throws Exception {
    XmlNode.Builder xmlNodeBuilder = XmlNode.newBuilder();
    TextFormat.merge(TestData.openReader("testdata/manifest/manifest1.textpb"), xmlNodeBuilder);
    AndroidManifest androidManifest = AndroidManifest.create(xmlNodeBuilder.build());

    XmlNode.Builder expectedXmlNodeBuilder = XmlNode.newBuilder();
    TextFormat.merge(
        TestData.openReader("testdata/manifest/config_split_manifest1.textpb"),
        expectedXmlNodeBuilder);

    AndroidManifest configManifest =
        AndroidManifest.createForConfigSplit(
            androidManifest.getPackageName(),
            androidManifest.getVersionCode(),
            "testModule.config.hdpi",
            "testModule",
            Optional.empty());

    XmlProtoNode generatedManifest = configManifest.getManifestRoot();
    assertThat(generatedManifest.getProto())
        .ignoringRepeatedFieldOrder()
        .isEqualTo(expectedXmlNodeBuilder.build());
  }

  @Test
  public void getMetadataResourceId_resourceAttributePresent() {
    AndroidManifest androidManifest =
        AndroidManifest.create(
            xmlNode(
                xmlElement(
                    "manifest",
                    xmlNode(
                        xmlElement("application", metadataWithResourceRef("metadata-key", 123))))));

    assertThat(androidManifest.getMetadataResourceId("metadata-key")).hasValue(123);
  }

  @Test
  public void getMetadataResourceId_resourceAttributeAbsent() {
    AndroidManifest androidManifest =
        AndroidManifest.create(
            xmlNode(
                xmlElement(
                    "manifest",
                    xmlNode(xmlElement("application", metadataWithValue("metadata-key", "123"))))));

    InvalidBundleException exception =
        assertThrows(
            InvalidBundleException.class,
            () -> androidManifest.getMetadataResourceId("metadata-key"));
    assertThat(exception)
        .hasMessageThat()
        .contains(
            "Missing expected attribute 'android:resource' for <meta-data> element 'metadata-key'");
  }

  @Test
  public void getMetadataValue() {
    AndroidManifest androidManifest =
        AndroidManifest.create(
            xmlNode(
                xmlElement(
                    "manifest",
                    xmlNode(xmlElement("application", metadataWithValue("metadata-key", "123"))))));
    assertThat(androidManifest.getMetadataValue("metadata-key")).hasValue("123");
  }

  @Test
  public void hasExplicitlyDefinedNativeActivities_noNativeActivities() {
    AndroidManifest manifest = AndroidManifest.create(androidManifest("com.package.test"));

    assertThat(manifest.hasExplicitlyDefinedNativeActivities()).isFalse();
  }

  @Test
  public void hasExplicitlyDefinedNativeActivities_someNativeActivities() {
    AndroidManifest manifest =
        AndroidManifest.create(
            androidManifest(
                "com.package.test",
                withNativeActivity("libA"),
                withNativeActivity("libB"),
                withNativeActivity("libA")));

    assertThat(manifest.hasExplicitlyDefinedNativeActivities()).isTrue();
  }

  @Test
  public void hasExplicitlyDefinedNativeActivities_someActivitiesButNotNative() {
    AndroidManifest manifest =
        AndroidManifest.create(
            androidManifest("com.package.test", withMainActivity("com.package.test.MainActivity")));

    assertThat(manifest.hasExplicitlyDefinedNativeActivities()).isFalse();
  }

  @Test
  public void getActivitiesByName() {
    AndroidManifest manifest =
        AndroidManifest.create(
            androidManifest(
                "com.package.test",
                withNativeActivity("libA"),
                withMainActivity("main"),
                withCustomThemeActivity("activity1", 123)));

    assertThat(manifest.getActivitiesByName()).hasSize(3);
    assertThat(manifest.getActivitiesByName().keySet())
        .containsExactly("main", "activity1", "android.app.NativeActivity");
  }

  @Test
  public void getActivitiesByName_nameDuplication() {
    AndroidManifest manifest =
        AndroidManifest.create(
            androidManifest(
                "com.package.test", withMainActivity("main"), withMainActivity("main")));

    assertThat(manifest.getActivitiesByName().keySet()).containsExactly("main");
    assertThat(manifest.getActivitiesByName().get("main")).hasSize(2);
  }

  @Test
  public void getMetadataValueAsInteger() {
    AndroidManifest androidManifest =
        AndroidManifest.create(
            xmlNode(
                xmlElement(
                    "manifest",
                    xmlNode(
                        xmlElement(
                            "application", metadataWithValueAsInteger("metadata-key", 123))))));
    assertThat(androidManifest.getMetadataValueAsInteger("metadata-key")).hasValue(123);
  }

  @Test
  public void getMetadataValueAsBoolean() {
    AndroidManifest androidManifest =
        AndroidManifest.create(
            xmlNode(
                xmlElement(
                    "manifest",
                    xmlNode(
                        xmlElement(
                            "application", metadataWithValueAsBoolean("metadata-key", true))))));
    assertThat(androidManifest.getMetadataValueAsBoolean("metadata-key")).hasValue(true);
  }

  @Test
  public void hasSharedUserId() {
    AndroidManifest androidManifest = AndroidManifest.create(androidManifest("com.test.app"));
    assertThat(androidManifest.hasSharedUserId()).isFalse();

    AndroidManifest androidManifest2 =
        AndroidManifest.create(androidManifest("com.test.app", withSharedUserId("shared_user_id")));
    assertThat(androidManifest2.hasSharedUserId()).isTrue();
  }

  @Test
  public void isHeadless_true() {
    AndroidManifest manifest =
        AndroidManifest.create(androidManifest("com.package.test", withNativeActivity("libA")));

    assertThat(manifest.isHeadless()).isTrue();
  }

  @Test
  public void isHeadless_mainPhoneActivity_false() {
    AndroidManifest manifest =
        AndroidManifest.create(
            androidManifest(
                "com.package.test",
                withMainActivity("com.package.test.MainActivity"),
                withNativeActivity("libA")));

    assertThat(manifest.isHeadless()).isFalse();
  }

  @Test
  public void isHeadless_mainTvActivity_false() {
    AndroidManifest manifest =
        AndroidManifest.create(
            androidManifest(
                "com.package.test", withMainTvActivity("com.package.test.MainActivity")));

    assertThat(manifest.isHeadless()).isFalse();
  }

  @Test
  public void isHeadless_false_activitiesWithSameName() {
    AndroidManifest manifest =
        AndroidManifest.create(
            androidManifest(
                "com.package.test",
                withMainActivity("com.package.test.Activity"),
                withMainActivity("com.package.test.Activity"),
                withNativeActivity("libA"),
                withNativeActivity("libA")));

    assertThat(manifest.isHeadless()).isFalse();
  }

  @Test
  public void hasMainActivity_true() {
    AndroidManifest manifest =
        AndroidManifest.create(
            androidManifest("com.package.test", withMainActivity(".MainActivity")));

    assertThat(manifest.hasMainActivity()).isTrue();
  }

  @Test
  public void hasMainActivity_false() {
    AndroidManifest manifest =
        AndroidManifest.create(
            androidManifest(
                "com.package.test",
                withMainTvActivity(".MainTvActivity"),
                withNativeActivity("libA")));

    assertThat(manifest.hasMainActivity()).isFalse();
  }

  @Test
  public void hasMainTvActivity_true() {
    AndroidManifest manifest =
        AndroidManifest.create(
            androidManifest("com.package.test", withMainTvActivity(".MainActivity")));

    assertThat(manifest.hasMainTvActivity()).isTrue();
  }

  @Test
  public void hasMainTvActivity_false() {
    AndroidManifest manifest =
        AndroidManifest.create(
            androidManifest(
                "com.package.test",
                withMainActivity(".MainTvActivity"),
                withNativeActivity("libA")));

    assertThat(manifest.hasMainTvActivity()).isFalse();
  }

  private XmlNode metadataWithValue(String key, String value) {
    return xmlNode(
        xmlElement(
            "meta-data",
            ImmutableList.of(
                xmlAttribute(ANDROID_NAMESPACE_URI, "name", NAME_RESOURCE_ID, key),
                xmlAttribute(ANDROID_NAMESPACE_URI, "value", VALUE_RESOURCE_ID, value))));
  }

  private XmlNode metadataWithValueAsInteger(String key, int value) {
    return xmlNode(
        xmlElement(
            "meta-data",
            ImmutableList.of(
                xmlAttribute(ANDROID_NAMESPACE_URI, "name", NAME_RESOURCE_ID, key),
                xmlDecimalIntegerAttribute(
                    ANDROID_NAMESPACE_URI, "value", VALUE_RESOURCE_ID, value))));
  }

  private XmlNode metadataWithValueAsBoolean(String key, boolean value) {
    return xmlNode(
        xmlElement(
            "meta-data",
            ImmutableList.of(
                xmlAttribute(ANDROID_NAMESPACE_URI, "name", NAME_RESOURCE_ID, key),
                xmlBooleanAttribute(ANDROID_NAMESPACE_URI, "value", VALUE_RESOURCE_ID, value))));
  }

  private XmlNode metadataWithResourceRef(String key, int resourceIdValue) {
    return xmlNode(
        xmlElement(
            "meta-data",
            ImmutableList.of(
                xmlAttribute(ANDROID_NAMESPACE_URI, "name", NAME_RESOURCE_ID, key),
                xmlResourceReferenceAttribute(
                    ANDROID_NAMESPACE_URI, "resource", RESOURCE_RESOURCE_ID, resourceIdValue))));
  }

  @Test
  public void getDeliveryType_noConfig() throws Exception {
    AndroidManifest manifest = AndroidManifest.create(androidManifest("com.test.app"));
    assertThat(manifest.getModuleDeliveryType()).isEqualTo(ALWAYS_INITIAL_INSTALL);
  }

  @Test
  public void getDeliveryType_legacy_onDemandTrue() throws Exception {
    AndroidManifest manifest =
        AndroidManifest.create(androidManifest("com.test.app", withOnDemandAttribute(true)));
    assertThat(manifest.getModuleDeliveryType()).isEqualTo(ModuleDeliveryType.NO_INITIAL_INSTALL);
  }

  @Test
  public void getDeliveryType_legacy_onDemandFalse() throws Exception {
    AndroidManifest manifest =
        AndroidManifest.create(androidManifest("com.test.app", withOnDemandAttribute(false)));
    assertThat(manifest.getModuleDeliveryType()).isEqualTo(ALWAYS_INITIAL_INSTALL);
  }

  @Test
  public void getDeliveryType_onDemandElement_only() throws Exception {
    AndroidManifest manifest =
        AndroidManifest.create(androidManifest("com.test.app", withOnDemandDelivery()));
    assertThat(manifest.getModuleDeliveryType()).isEqualTo(ModuleDeliveryType.NO_INITIAL_INSTALL);
  }

  @Test
  public void getDeliveryType_onDemandElement_andConditions() throws Exception {
    AndroidManifest manifest =
        AndroidManifest.create(
            androidManifest("com.test.app", withOnDemandDelivery(), withMinSdkCondition(21)));
    assertThat(manifest.getModuleDeliveryType())
        .isEqualTo(ModuleDeliveryType.CONDITIONAL_INITIAL_INSTALL);
  }

  @Test
  public void getDeliveryType_installTimeElement_noConditions() throws Exception {
    AndroidManifest manifest =
        AndroidManifest.create(
            androidManifest("com.test.app", withInstallTimeDelivery(), withOnDemandDelivery()));
    assertThat(manifest.getModuleDeliveryType()).isEqualTo(ALWAYS_INITIAL_INSTALL);
  }

  @Test
  public void getInstantDeliveryType_instantAttributeTrue() {
    AndroidManifest manifest =
        AndroidManifest.create(androidManifestForAssetModule("com.test.app", withInstant(true)));
    assertThat(manifest.getInstantModuleDeliveryType()).isEqualTo(NO_INITIAL_INSTALL);
  }

  @Test
  public void getInstantDeliveryType_onDemandElement() {
    AndroidManifest manifest =
        AndroidManifest.create(
            androidManifestForAssetModule("com.test.app", withInstantOnDemandDelivery()));
    assertThat(manifest.getInstantModuleDeliveryType()).isEqualTo(NO_INITIAL_INSTALL);
  }

  @Test
  public void getInstantDeliveryType_installTimeElement() {
    AndroidManifest manifest =
        AndroidManifest.create(
            androidManifestForAssetModule("com.test.app", withInstantInstallTimeDelivery()));
    assertThat(manifest.getInstantModuleDeliveryType()).isEqualTo(ALWAYS_INITIAL_INSTALL);
  }

  @Test
  public void hasApplicationAttribute_present() {
    AndroidManifest androidManifest =
        createManifestWithApplicationAttribute("myAttributeName", 0x12341234, "value");

    assertThat(androidManifest.hasApplicationAttribute(0x12341234)).isTrue();
  }

  @Test
  public void hasApplicationAttribute_missing_returnsFalse() {
    AndroidManifest androidManifest =
        AndroidManifest.create(xmlNode(xmlElement("manifest", xmlNode(xmlElement("application")))));

    assertThat(androidManifest.hasApplicationAttribute(0x12341234)).isFalse();
  }

  @Test
  public void getPermissions_success() {
    XmlProtoElement permission1 =
        new XmlProtoElement(
            xmlElement(
                PERMISSION_ELEMENT_NAME,
                xmlAttribute(ANDROID_NAMESPACE_URI, "name", NAME_RESOURCE_ID, "SEND_SMS")));
    XmlProtoElement permission2 =
        new XmlProtoElement(
            xmlElement(
                PERMISSION_ELEMENT_NAME,
                ImmutableList.of(
                    xmlAttribute(
                        ANDROID_NAMESPACE_URI,
                        "name",
                        NAME_RESOURCE_ID,
                        "com.some.other.PERMISSION"),
                    xmlResourceReferenceAttribute(
                        ANDROID_NAMESPACE_URI, ICON_ATTRIBUTE_NAME, ICON_RESOURCE_ID, 12341234),
                    xmlResourceReferenceAttribute(
                        ANDROID_NAMESPACE_URI, LABEL_ATTRIBUTE_NAME, LABEL_RESOURCE_ID, 0x12345678),
                    xmlResourceReferenceAttribute(
                        ANDROID_NAMESPACE_URI,
                        DESCRIPTION_ATTRIBUTE_NAME,
                        DESCRIPTION_RESOURCE_ID,
                        0x87654321),
                    xmlAttribute(ANDROID_NAMESPACE_URI, "protectionLevel", "normal|signature"),
                    xmlAttribute(ANDROID_NAMESPACE_URI, "permissionGroup", "group1"))));
    AndroidManifest androidManifest =
        AndroidManifest.create(
            xmlNode(
                xmlElement(
                    "manifest", xmlNode(permission1.getProto()), xmlNode(permission2.getProto()))));

    assertThat(androidManifest.getPermissions()).containsExactly(permission1, permission2);
  }

  @Test
  public void getPermissionGroups_success() {
    XmlProtoElement permisisonGroup1 =
        new XmlProtoElement(
            xmlElement(
                PERMISSION_GROUP_ELEMENT_NAME,
                xmlAttribute(ANDROID_NAMESPACE_URI, "name", NAME_RESOURCE_ID, "group.name.1")));
    XmlProtoElement permisisonGroup2 =
        new XmlProtoElement(
            xmlElement(
                PERMISSION_GROUP_ELEMENT_NAME,
                ImmutableList.of(
                    xmlAttribute(ANDROID_NAMESPACE_URI, "name", NAME_RESOURCE_ID, "group.name.2"),
                    xmlResourceReferenceAttribute(
                        ANDROID_NAMESPACE_URI, ICON_ATTRIBUTE_NAME, ICON_RESOURCE_ID, 12341234),
                    xmlResourceReferenceAttribute(
                        ANDROID_NAMESPACE_URI, LABEL_ATTRIBUTE_NAME, LABEL_RESOURCE_ID, 0x12345678),
                    xmlResourceReferenceAttribute(
                        ANDROID_NAMESPACE_URI,
                        DESCRIPTION_ATTRIBUTE_NAME,
                        DESCRIPTION_RESOURCE_ID,
                        0x87654321))));
    AndroidManifest androidManifest =
        AndroidManifest.create(
            xmlNode(
                xmlElement(
                    "manifest",
                    xmlNode(permisisonGroup1.getProto()),
                    xmlNode(permisisonGroup2.getProto()))));

    assertThat(androidManifest.getPermissionGroups())
        .containsExactly(permisisonGroup1, permisisonGroup2);
  }

  @Test
  public void getPermissions_isEmpty() {
    AndroidManifest androidManifest =
        AndroidManifest.create(xmlNode(xmlElement("manifest", xmlNode(xmlElement("application")))));

    assertThat(androidManifest.getPermissionGroups()).isEmpty();
  }

  private AndroidManifest createManifestWithApplicationAttribute(
      String name, int resourceId, String value) {
    return AndroidManifest.create(
        xmlNode(
            xmlElement(
                "manifest",
                xmlNode(
                    xmlElement(
                        "application",
                        xmlAttribute(ANDROID_NAMESPACE_URI, name, resourceId, value))))));
  }

  private AndroidManifest createManifestWithApplicationRefIdAttribute(
      String name, int resourceId, int value) {
    return AndroidManifest.create(
        xmlNode(
            xmlElement(
                "manifest",
                xmlNode(
                    xmlElement(
                        "application",
                        xmlResourceReferenceAttribute(
                            ANDROID_NAMESPACE_URI, name, resourceId, value))))));
  }

  @Test
  public void hasLocaleConfig_missing_returnsFalse() {
    AndroidManifest androidManifest =
        AndroidManifest.create(xmlNode(xmlElement("manifest", xmlNode(xmlElement("application")))));

    assertThat(androidManifest.hasLocaleConfig()).isFalse();
  }

  @Test
  public void hasLocaleConfig_present() {
    AndroidManifest androidManifest =
        createManifestWithApplicationRefIdAttribute(
            LOCALE_CONFIG_ATTRIBUTE_NAME, LOCALE_CONFIG_RESOURCE_ID, 0x12345678);

    assertThat(androidManifest.hasLocaleConfig()).isTrue();
  }

  @Test
  public void getAppComponentFactoryAttribute_returnsEmpty() {
    AndroidManifest androidManifest =
        AndroidManifest.create(xmlNode(xmlElement("manifest", xmlNode(xmlElement("application")))));

    assertThat(androidManifest.getAppComponentFactoryAttribute()).isEmpty();
  }

  @Test
  public void getAppComponentFactoryAttribute_present() {
    AndroidManifest androidManifest =
        AndroidManifest.create(
            xmlNode(
                xmlElement(
                    "manifest",
                    xmlNode(
                        xmlElement(
                            "application",
                            xmlAttribute(
                                ANDROID_NAMESPACE_URI,
                                APP_COMPONENT_FACTORY_ATTRIBUTE_NAME,
                                APP_COMPONENT_FACTORY_RESOURCE_ID,
                                "my.package.customFactory"))))));

    assertThat(androidManifest.getAppComponentFactoryAttribute())
        .hasValue("my.package.customFactory");
  }

  @Test
  public void getAuthoritiesAttribute_present() {
    AndroidManifest androidManifest =
        AndroidManifest.create(
            xmlNode(
                xmlElement(
                    "manifest",
                    xmlNode(
                        xmlElement(
                            "application",
                            xmlAttribute(
                                ANDROID_NAMESPACE_URI,
                                AUTHORITIES_ATTRIBUTE_NAME,
                                AUTHORITIES_RESOURCE_ID,
                                "my.package.customAuthority"))))));

    assertThat(androidManifest.getAuthoritiesAttribute()).hasValue("my.package.customAuthority");
  }

  @Test
  public void getUsesFeatureElement_present() {
    XmlElement usesFeatureElement =
        xmlElement(
            "uses-feature",
            xmlAttribute(
                ANDROID_NAMESPACE_URI, NAME_ATTRIBUTE_NAME, NAME_RESOURCE_ID, "featureName"));
    AndroidManifest androidManifest =
        AndroidManifest.create(xmlNode(xmlElement("manifest", xmlNode(usesFeatureElement))));

    assertThat(androidManifest.getUsesFeatureElement("featureName"))
        .containsExactlyElementsIn(ImmutableList.of(new XmlProtoElement(usesFeatureElement)));
  }

  @Test
  public void getUsesFeatureElement_absent() {
    AndroidManifest androidManifest = AndroidManifest.create(xmlNode(xmlElement("manifest")));

    assertThat(androidManifest.getUsesFeatureElement("featureName")).isEmpty();
  }

  @Test
  public void isAlwaysInstalledModule_legacyOnDemandTrue_returnsFalse() {
    AndroidManifest manifest =
        AndroidManifest.create(androidManifest("com.test.app", withOnDemandAttribute(true)));
    assertThat(manifest.isAlwaysInstalledModule()).isFalse();
  }

  @Test
  public void isAlwaysInstalledModule_legacyOnDemandFalse_returnsTrue() {
    AndroidManifest manifest =
        AndroidManifest.create(androidManifest("com.test.app", withOnDemandAttribute(false)));
    assertThat(manifest.isAlwaysInstalledModule()).isTrue();
  }

  @Test
  public void isAlwaysInstalledModule_noDeliveryElements_returnsTrue() {
    AndroidManifest manifest = AndroidManifest.create(xmlNode(xmlElement("manifest")));
    assertThat(manifest.isAlwaysInstalledModule()).isTrue();
  }

  @Test
  public void isAlwaysInstalledModule_moduleConditions_returnsFalse() {
    AndroidManifest manifest =
        AndroidManifest.create(
            androidManifest("com.test.app", withFeatureCondition("com.feature1")));
    assertThat(manifest.isAlwaysInstalledModule()).isFalse();
  }

  @Test
  public void isAlwaysInstalledModule_minSdkCondition_returnsFalse() {
    AndroidManifest manifest =
        AndroidManifest.create(androidManifest("com.test.app", withMinSdkCondition(21)));
    assertThat(manifest.isAlwaysInstalledModule()).isFalse();
  }

  @Test
  public void isAlwaysInstalledModule_onDemandElement_returnsFalse() {
    AndroidManifest manifest =
        AndroidManifest.create(androidManifest("com.test.app", withOnDemandDelivery()));
    assertThat(manifest.isAlwaysInstalledModule()).isFalse();
  }

  @Test
  public void isAlwaysInstalledModule_fastFollow_returnsFalse() {
    AndroidManifest manifest =
        AndroidManifest.create(
            androidManifest(
                "com.test.app",
                withTypeAttribute(MODULE_TYPE_ASSET_VALUE),
                withFastFollowDelivery()));
    assertThat(manifest.isAlwaysInstalledModule()).isFalse();
  }

  @Test
  public void isAlwaysInstalledModule_installTimeRemovableFalse_returnsTrue() {
    AndroidManifest manifest =
        AndroidManifest.create(
            androidManifest("com.test.app", withInstallTimeRemovableElement(false)));
    assertThat(manifest.isAlwaysInstalledModule()).isTrue();
  }

  @Test
  public void isAlwaysInstalledModule_installTimeRemovableTrue_returnsFalse() {
    AndroidManifest manifest =
        AndroidManifest.create(
            androidManifest("com.test.app", withInstallTimeRemovableElement(true)));
    assertThat(manifest.isAlwaysInstalledModule()).isFalse();
  }

  @Test
  public void isAlwaysInstalledModule_installTimeNoRemovable_returnsTrue() {
    AndroidManifest manifest =
        AndroidManifest.create(androidManifest("com.test.app", withInstallTimeDelivery()));
    assertThat(manifest.isAlwaysInstalledModule()).isTrue();
  }
}
