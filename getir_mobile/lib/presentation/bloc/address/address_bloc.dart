import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:equatable/equatable.dart';
import '../../../core/errors/app_exceptions.dart';
import '../../../domain/entities/address.dart';
import '../../../domain/usecases/address_usecases.dart';
import '../../../data/datasources/address_datasource.dart';

// Events
abstract class AddressEvent extends Equatable {
  const AddressEvent();

  @override
  List<Object?> get props => [];
}

class LoadUserAddresses extends AddressEvent {}

class LoadAddressById extends AddressEvent {
  final String addressId;

  const LoadAddressById(this.addressId);

  @override
  List<Object> get props => [addressId];
}

class CreateAddress extends AddressEvent {
  final CreateAddressRequest request;

  const CreateAddress(this.request);

  @override
  List<Object> get props => [request];
}

class UpdateAddress extends AddressEvent {
  final String addressId;
  final UpdateAddressRequest request;

  const UpdateAddress(this.addressId, this.request);

  @override
  List<Object> get props => [addressId, request];
}

class DeleteAddress extends AddressEvent {
  final String addressId;

  const DeleteAddress(this.addressId);

  @override
  List<Object> get props => [addressId];
}

class SetDefaultAddress extends AddressEvent {
  final String addressId;

  const SetDefaultAddress(this.addressId);

  @override
  List<Object> get props => [addressId];
}

// States
abstract class AddressState extends Equatable {
  const AddressState();

  @override
  List<Object?> get props => [];
}

class AddressInitial extends AddressState {}

class AddressLoading extends AddressState {}

class AddressesLoaded extends AddressState {
  final List<UserAddress> addresses;

  const AddressesLoaded(this.addresses);

  @override
  List<Object> get props => [addresses];
}

class AddressLoaded extends AddressState {
  final UserAddress address;

  const AddressLoaded(this.address);

  @override
  List<Object> get props => [address];
}

class AddressCreated extends AddressState {
  final UserAddress address;

  const AddressCreated(this.address);

  @override
  List<Object> get props => [address];
}

class AddressUpdated extends AddressState {
  final UserAddress address;

  const AddressUpdated(this.address);

  @override
  List<Object> get props => [address];
}

class AddressDeleted extends AddressState {
  final String addressId;

  const AddressDeleted(this.addressId);

  @override
  List<Object> get props => [addressId];
}

class DefaultAddressSet extends AddressState {
  final UserAddress address;

  const DefaultAddressSet(this.address);

  @override
  List<Object> get props => [address];
}

class AddressError extends AddressState {
  final String message;

  const AddressError(this.message);

  @override
  List<Object> get props => [message];
}

// BLoC
class AddressBloc extends Bloc<AddressEvent, AddressState> {
  final GetUserAddressesUseCase _getUserAddressesUseCase;
  final GetAddressByIdUseCase _getAddressByIdUseCase;
  final CreateAddressUseCase _createAddressUseCase;
  final UpdateAddressUseCase _updateAddressUseCase;
  final DeleteAddressUseCase _deleteAddressUseCase;
  final SetDefaultAddressUseCase _setDefaultAddressUseCase;

  AddressBloc({
    required GetUserAddressesUseCase getUserAddressesUseCase,
    required GetAddressByIdUseCase getAddressByIdUseCase,
    required CreateAddressUseCase createAddressUseCase,
    required UpdateAddressUseCase updateAddressUseCase,
    required DeleteAddressUseCase deleteAddressUseCase,
    required SetDefaultAddressUseCase setDefaultAddressUseCase,
  }) : _getUserAddressesUseCase = getUserAddressesUseCase,
       _getAddressByIdUseCase = getAddressByIdUseCase,
       _createAddressUseCase = createAddressUseCase,
       _updateAddressUseCase = updateAddressUseCase,
       _deleteAddressUseCase = deleteAddressUseCase,
       _setDefaultAddressUseCase = setDefaultAddressUseCase,
       super(AddressInitial()) {
    on<LoadUserAddresses>(_onLoadUserAddresses);
    on<LoadAddressById>(_onLoadAddressById);
    on<CreateAddress>(_onCreateAddress);
    on<UpdateAddress>(_onUpdateAddress);
    on<DeleteAddress>(_onDeleteAddress);
    on<SetDefaultAddress>(_onSetDefaultAddress);
  }

  Future<void> _onLoadUserAddresses(
    LoadUserAddresses event,
    Emitter<AddressState> emit,
  ) async {
    emit(AddressLoading());

    final result = await _getUserAddressesUseCase();

    result.when(
      success: (addresses) => emit(AddressesLoaded(addresses)),
      failure: (exception) {
        final message = _getErrorMessage(exception);
        emit(AddressError(message));
      },
    );
  }

  Future<void> _onLoadAddressById(
    LoadAddressById event,
    Emitter<AddressState> emit,
  ) async {
    emit(AddressLoading());

    final result = await _getAddressByIdUseCase(event.addressId);

    result.when(
      success: (address) => emit(AddressLoaded(address)),
      failure: (exception) {
        final message = _getErrorMessage(exception);
        emit(AddressError(message));
      },
    );
  }

  Future<void> _onCreateAddress(
    CreateAddress event,
    Emitter<AddressState> emit,
  ) async {
    emit(AddressLoading());

    final result = await _createAddressUseCase(event.request);

    result.when(
      success: (address) => emit(AddressCreated(address)),
      failure: (exception) {
        final message = _getErrorMessage(exception);
        emit(AddressError(message));
      },
    );
  }

  Future<void> _onUpdateAddress(
    UpdateAddress event,
    Emitter<AddressState> emit,
  ) async {
    emit(AddressLoading());

    final result = await _updateAddressUseCase(event.addressId, event.request);

    result.when(
      success: (address) => emit(AddressUpdated(address)),
      failure: (exception) {
        final message = _getErrorMessage(exception);
        emit(AddressError(message));
      },
    );
  }

  Future<void> _onDeleteAddress(
    DeleteAddress event,
    Emitter<AddressState> emit,
  ) async {
    emit(AddressLoading());

    final result = await _deleteAddressUseCase(event.addressId);

    result.when(
      success: (_) => emit(AddressDeleted(event.addressId)),
      failure: (exception) {
        final message = _getErrorMessage(exception);
        emit(AddressError(message));
      },
    );
  }

  Future<void> _onSetDefaultAddress(
    SetDefaultAddress event,
    Emitter<AddressState> emit,
  ) async {
    emit(AddressLoading());

    final result = await _setDefaultAddressUseCase(event.addressId);

    result.when(
      success: (address) => emit(DefaultAddressSet(address)),
      failure: (exception) {
        final message = _getErrorMessage(exception);
        emit(AddressError(message));
      },
    );
  }

  /// Extract user-friendly error message from exception
  String _getErrorMessage(Exception exception) {
    if (exception is AppException) {
      return exception.message;
    }
    return 'An unexpected error occurred';
  }
}
