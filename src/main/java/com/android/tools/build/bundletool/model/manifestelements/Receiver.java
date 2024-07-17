/*
 * Copyright (C) 2021 The Android Open Source Project
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

package com.android.tools.build.bundletool.model.manifestelements;

import static com.android.tools.build.bundletool.model.AndroidManifest.EXPORTED_ATTRIBUTE_NAME;
import static com.android.tools.build.bundletool.model.AndroidManifest.EXPORTED_RESOURCE_ID;
import static com.android.tools.build.bundletool.model.AndroidManifest.NAME_ATTRIBUTE_NAME;
import static com.android.tools.build.bundletool.model.AndroidManifest.NAME_RESOURCE_ID;
import static com.android.tools.build.bundletool.model.AndroidManifest.RECEIVER_ELEMENT_NAME;

import com.android.tools.build.bundletool.model.utils.xmlproto.XmlProtoElement;
import com.android.tools.build.bundletool.model.utils.xmlproto.XmlProtoElementBuilder;
import com.google.auto.value.AutoValue;
import com.google.auto.value.extension.memoized.Memoized;
import com.google.errorprone.annotations.Immutable;
import java.util.Optional;

/**
 * Represents Reciever element of Android manifest.
 *
 * <p>This is not an exhaustive representation, some attributes and child elements might be missing.
 */
@Immutable
@AutoValue
@AutoValue.CopyAnnotations
public abstract class Receiver {
  abstract Optional<String> getName();

  abstract Optional<Boolean> getExported();

  abstract Optional<IntentFilter> getIntentFilter();

  public static Builder builder() {
    return new AutoValue_Receiver.Builder();
  }

  @Memoized
  public XmlProtoElement asXmlProtoElement() {
    XmlProtoElementBuilder elementBuilder = XmlProtoElementBuilder.create(RECEIVER_ELEMENT_NAME);
    setNameAttribute(elementBuilder);
    setExportedAttribute(elementBuilder);
    setIntentFilterElement(elementBuilder);
    return elementBuilder.build();
  }

  private void setNameAttribute(XmlProtoElementBuilder elementBuilder) {
    if (getName().isPresent()) {
      elementBuilder
          .getOrCreateAndroidAttribute(NAME_ATTRIBUTE_NAME, NAME_RESOURCE_ID)
          .setValueAsString(getName().get());
    }
  }

  private void setExportedAttribute(XmlProtoElementBuilder elementBuilder) {
    if (getExported().isPresent()) {
      elementBuilder
          .getOrCreateAndroidAttribute(EXPORTED_ATTRIBUTE_NAME, EXPORTED_RESOURCE_ID)
          .setValueAsBoolean(getExported().get());
    }
  }

  private void setIntentFilterElement(XmlProtoElementBuilder elementBuilder) {
    if (getIntentFilter().isPresent()) {
      elementBuilder.addChildElement(getIntentFilter().get().asXmlProtoElement().toBuilder());
    }
  }

  /** Builder for Receiever. */
  @AutoValue.Builder
  public abstract static class Builder {
    public abstract Builder setName(String name);

    public abstract Builder setExported(boolean exported);

    public abstract Builder setIntentFilter(IntentFilter intentFilter);

    public abstract Receiver build();
  }
}
