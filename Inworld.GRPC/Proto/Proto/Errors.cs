// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: ai/inworld/studio/v1alpha/errors.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021, 8981
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Ai.Inworld.Studio.V1Alpha {

  /// <summary>Holder for reflection information generated from ai/inworld/studio/v1alpha/errors.proto</summary>
  public static partial class ErrorsReflection {

    #region Descriptor
    /// <summary>File descriptor for ai/inworld/studio/v1alpha/errors.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static ErrorsReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "CiZhaS9pbndvcmxkL3N0dWRpby92MWFscGhhL2Vycm9ycy5wcm90bxIZYWku",
            "aW53b3JsZC5zdHVkaW8udjFhbHBoYSKaAgoKQmFkUmVxdWVzdBJOChBmaWVs",
            "ZF92aW9sYXRpb25zGAEgAygLMjQuYWkuaW53b3JsZC5zdHVkaW8udjFhbHBo",
            "YS5CYWRSZXF1ZXN0LkZpZWxkVmlvbGF0aW9uGrsBCg5GaWVsZFZpb2xhdGlv",
            "bhINCgVmaWVsZBgBIAEoCRITCgtkZXNjcmlwdGlvbhgCIAEoCRJUCghtZXRh",
            "ZGF0YRgDIAMoCzJCLmFpLmlud29ybGQuc3R1ZGlvLnYxYWxwaGEuQmFkUmVx",
            "dWVzdC5GaWVsZFZpb2xhdGlvbi5NZXRhZGF0YUVudHJ5Gi8KDU1ldGFkYXRh",
            "RW50cnkSCwoDa2V5GAEgASgJEg0KBXZhbHVlGAIgASgJOgI4AUKVAQoZYWku",
            "aW53b3JsZC5zdHVkaW8udjFhbHBoYUILRXJyb3JzUHJvdG9QAVpNZ2l0aHVi",
            "LmNvbS9pbndvcmxkLWFpL2lud29ybGQvc2VydmluZy9ncnBjLWdhdGV3YXkv",
            "YnVpbGQvcHJvdG8vc3R1ZGlvL3YxYWxwaGGqAhlBaS5JbndvcmxkLlN0dWRp",
            "by5WMUFscGhhYgZwcm90bzM="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Ai.Inworld.Studio.V1Alpha.BadRequest), global::Ai.Inworld.Studio.V1Alpha.BadRequest.Parser, new[]{ "FieldViolations" }, null, null, null, new pbr::GeneratedClrTypeInfo[] { new pbr::GeneratedClrTypeInfo(typeof(global::Ai.Inworld.Studio.V1Alpha.BadRequest.Types.FieldViolation), global::Ai.Inworld.Studio.V1Alpha.BadRequest.Types.FieldViolation.Parser, new[]{ "Field", "Description", "Metadata" }, null, null, null, new pbr::GeneratedClrTypeInfo[] { null, })})
          }));
    }
    #endregion

  }
  #region Messages
  /// <summary>
  /// Describes violations in a client request. This error type focuses on the
  /// syntactic aspects of the request.
  /// </summary>
  public sealed partial class BadRequest : pb::IMessage<BadRequest>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<BadRequest> _parser = new pb::MessageParser<BadRequest>(() => new BadRequest());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<BadRequest> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Ai.Inworld.Studio.V1Alpha.ErrorsReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public BadRequest() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public BadRequest(BadRequest other) : this() {
      fieldViolations_ = other.fieldViolations_.Clone();
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public BadRequest Clone() {
      return new BadRequest(this);
    }

    /// <summary>Field number for the "field_violations" field.</summary>
    public const int FieldViolationsFieldNumber = 1;
    private static readonly pb::FieldCodec<global::Ai.Inworld.Studio.V1Alpha.BadRequest.Types.FieldViolation> _repeated_fieldViolations_codec
        = pb::FieldCodec.ForMessage(10, global::Ai.Inworld.Studio.V1Alpha.BadRequest.Types.FieldViolation.Parser);
    private readonly pbc::RepeatedField<global::Ai.Inworld.Studio.V1Alpha.BadRequest.Types.FieldViolation> fieldViolations_ = new pbc::RepeatedField<global::Ai.Inworld.Studio.V1Alpha.BadRequest.Types.FieldViolation>();
    /// <summary>
    /// Describes all violations in a client request.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public pbc::RepeatedField<global::Ai.Inworld.Studio.V1Alpha.BadRequest.Types.FieldViolation> FieldViolations {
      get { return fieldViolations_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as BadRequest);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(BadRequest other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if(!fieldViolations_.Equals(other.fieldViolations_)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      hash ^= fieldViolations_.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      fieldViolations_.WriteTo(output, _repeated_fieldViolations_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      fieldViolations_.WriteTo(ref output, _repeated_fieldViolations_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      size += fieldViolations_.CalculateSize(_repeated_fieldViolations_codec);
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(BadRequest other) {
      if (other == null) {
        return;
      }
      fieldViolations_.Add(other.fieldViolations_);
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            fieldViolations_.AddEntriesFrom(input, _repeated_fieldViolations_codec);
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 10: {
            fieldViolations_.AddEntriesFrom(ref input, _repeated_fieldViolations_codec);
            break;
          }
        }
      }
    }
    #endif

    #region Nested types
    /// <summary>Container for nested types declared in the BadRequest message type.</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static partial class Types {
      /// <summary>
      /// A message type used to describe a single bad request field.
      /// </summary>
      public sealed partial class FieldViolation : pb::IMessage<FieldViolation>
      #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
          , pb::IBufferMessage
      #endif
      {
        private static readonly pb::MessageParser<FieldViolation> _parser = new pb::MessageParser<FieldViolation>(() => new FieldViolation());
        private pb::UnknownFieldSet _unknownFields;
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public static pb::MessageParser<FieldViolation> Parser { get { return _parser; } }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public static pbr::MessageDescriptor Descriptor {
          get { return global::Ai.Inworld.Studio.V1Alpha.BadRequest.Descriptor.NestedTypes[0]; }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        pbr::MessageDescriptor pb::IMessage.Descriptor {
          get { return Descriptor; }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public FieldViolation() {
          OnConstruction();
        }

        partial void OnConstruction();

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public FieldViolation(FieldViolation other) : this() {
          field_ = other.field_;
          description_ = other.description_;
          metadata_ = other.metadata_.Clone();
          _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public FieldViolation Clone() {
          return new FieldViolation(this);
        }

        /// <summary>Field number for the "field" field.</summary>
        public const int FieldFieldNumber = 1;
        private string field_ = "";
        /// <summary>
        /// A path leading to a field in the request body. The value will be a
        /// sequence of dot-separated identifiers that identify a field. E.g.,
        /// "field_violations.field" would identify this field.
        /// For repeated case the indexes is used in names to specify
        /// which one is incorrect
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public string Field {
          get { return field_; }
          set {
            field_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
          }
        }

        /// <summary>Field number for the "description" field.</summary>
        public const int DescriptionFieldNumber = 2;
        private string description_ = "";
        /// <summary>
        /// A description of why the request element is bad.
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public string Description {
          get { return description_; }
          set {
            description_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
          }
        }

        /// <summary>Field number for the "metadata" field.</summary>
        public const int MetadataFieldNumber = 3;
        private static readonly pbc::MapField<string, string>.Codec _map_metadata_codec
            = new pbc::MapField<string, string>.Codec(pb::FieldCodec.ForString(10, ""), pb::FieldCodec.ForString(18, ""), 26);
        private readonly pbc::MapField<string, string> metadata_ = new pbc::MapField<string, string>();
        /// <summary>
        /// to send specific details for errors. E.g. type for character error
        /// </summary>
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public pbc::MapField<string, string> Metadata {
          get { return metadata_; }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public override bool Equals(object other) {
          return Equals(other as FieldViolation);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public bool Equals(FieldViolation other) {
          if (ReferenceEquals(other, null)) {
            return false;
          }
          if (ReferenceEquals(other, this)) {
            return true;
          }
          if (Field != other.Field) return false;
          if (Description != other.Description) return false;
          if (!Metadata.Equals(other.Metadata)) return false;
          return Equals(_unknownFields, other._unknownFields);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public override int GetHashCode() {
          int hash = 1;
          if (Field.Length != 0) hash ^= Field.GetHashCode();
          if (Description.Length != 0) hash ^= Description.GetHashCode();
          hash ^= Metadata.GetHashCode();
          if (_unknownFields != null) {
            hash ^= _unknownFields.GetHashCode();
          }
          return hash;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public override string ToString() {
          return pb::JsonFormatter.ToDiagnosticString(this);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public void WriteTo(pb::CodedOutputStream output) {
        #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
          output.WriteRawMessage(this);
        #else
          if (Field.Length != 0) {
            output.WriteRawTag(10);
            output.WriteString(Field);
          }
          if (Description.Length != 0) {
            output.WriteRawTag(18);
            output.WriteString(Description);
          }
          metadata_.WriteTo(output, _map_metadata_codec);
          if (_unknownFields != null) {
            _unknownFields.WriteTo(output);
          }
        #endif
        }

        #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
          if (Field.Length != 0) {
            output.WriteRawTag(10);
            output.WriteString(Field);
          }
          if (Description.Length != 0) {
            output.WriteRawTag(18);
            output.WriteString(Description);
          }
          metadata_.WriteTo(ref output, _map_metadata_codec);
          if (_unknownFields != null) {
            _unknownFields.WriteTo(ref output);
          }
        }
        #endif

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public int CalculateSize() {
          int size = 0;
          if (Field.Length != 0) {
            size += 1 + pb::CodedOutputStream.ComputeStringSize(Field);
          }
          if (Description.Length != 0) {
            size += 1 + pb::CodedOutputStream.ComputeStringSize(Description);
          }
          size += metadata_.CalculateSize(_map_metadata_codec);
          if (_unknownFields != null) {
            size += _unknownFields.CalculateSize();
          }
          return size;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public void MergeFrom(FieldViolation other) {
          if (other == null) {
            return;
          }
          if (other.Field.Length != 0) {
            Field = other.Field;
          }
          if (other.Description.Length != 0) {
            Description = other.Description;
          }
          metadata_.Add(other.metadata_);
          _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        public void MergeFrom(pb::CodedInputStream input) {
        #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
          input.ReadRawMessage(this);
        #else
          uint tag;
          while ((tag = input.ReadTag()) != 0) {
            switch(tag) {
              default:
                _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
                break;
              case 10: {
                Field = input.ReadString();
                break;
              }
              case 18: {
                Description = input.ReadString();
                break;
              }
              case 26: {
                metadata_.AddEntriesFrom(input, _map_metadata_codec);
                break;
              }
            }
          }
        #endif
        }

        #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
        void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
          uint tag;
          while ((tag = input.ReadTag()) != 0) {
            switch(tag) {
              default:
                _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
                break;
              case 10: {
                Field = input.ReadString();
                break;
              }
              case 18: {
                Description = input.ReadString();
                break;
              }
              case 26: {
                metadata_.AddEntriesFrom(ref input, _map_metadata_codec);
                break;
              }
            }
          }
        }
        #endif

      }

    }
    #endregion

  }

  #endregion

}

#endregion Designer generated code